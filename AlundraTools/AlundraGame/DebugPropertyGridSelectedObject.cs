using AlundraEngine.DatasBin;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace AlundraGame;

// JUSTIFICATION: backend MonoGame only
internal sealed class DebugPropertyGridSelectedObject : ICustomTypeDescriptor
{
    public const string ShiftedDescriptorName = "ShiftedFieldDescriptor";
    public const string MapTilesDescriptorName = "MapTilesFieldDescriptor";

    private object _instance;
    private readonly IReadOnlyDictionary<string, string> _categories;
    private readonly IReadOnlyDictionary<string, string> _descriptors;
    private readonly Dictionary<string, int> _categoryOrder;
    private PropertyDescriptorCollection? _properties;

    public DebugPropertyGridSelectedObject(
        object instance,
        IReadOnlyDictionary<string, string> categories,
        IReadOnlyDictionary<string, string> descriptors)
    {
        _instance = instance ?? throw new ArgumentNullException(nameof(instance));
        _categories = categories ?? throw new ArgumentNullException(nameof(categories));
        _descriptors = descriptors ?? throw new ArgumentNullException(nameof(descriptors));
        _categoryOrder = CreateCategoryOrder(categories);
    }

    public bool CanReuseFor(
        object instance,
        IReadOnlyDictionary<string, string> categories,
        IReadOnlyDictionary<string, string> descriptors)
    {
        return instance != null
            && _instance.GetType() == instance.GetType()
            && ReferenceEquals(_categories, categories)
            && ReferenceEquals(_descriptors, descriptors);
    }

    public void SetInstance(object instance)
    {
        if (instance == null)
        {
            throw new ArgumentNullException(nameof(instance));
        }

        if (_instance.GetType() != instance.GetType())
        {
            throw new InvalidOperationException("Cannot reuse a property-grid selected object across different runtime types.");
        }

        _instance = instance;
    }

    public AttributeCollection GetAttributes() => TypeDescriptor.GetAttributes(_instance);
    public string? GetClassName() => TypeDescriptor.GetClassName(_instance);
    public string? GetComponentName() => TypeDescriptor.GetComponentName(_instance);
    public TypeConverter GetConverter() => TypeDescriptor.GetConverter(_instance);
    public EventDescriptor? GetDefaultEvent() => TypeDescriptor.GetDefaultEvent(_instance);
    public PropertyDescriptor? GetDefaultProperty() => TypeDescriptor.GetDefaultProperty(_instance);
    public object? GetEditor(Type editorBaseType) => TypeDescriptor.GetEditor(_instance, editorBaseType);
    public EventDescriptorCollection GetEvents() => TypeDescriptor.GetEvents(_instance);
    public EventDescriptorCollection GetEvents(Attribute[]? attributes) => TypeDescriptor.GetEvents(_instance, attributes ?? Array.Empty<Attribute>());
    public object GetPropertyOwner(PropertyDescriptor? pd) => this;

    public PropertyDescriptorCollection GetProperties() => GetProperties(null);

    public PropertyDescriptorCollection GetProperties(Attribute[]? attributes)
    {
        _properties ??= BuildProperties();
        return _properties;
    }

    private PropertyDescriptorCollection BuildProperties()
    {
        List<FormattedMemberDescriptor> descriptors = new();

        foreach (MemberInfo member in GetSortedMembers())
        {
            string descriptorName = _descriptors.TryGetValue(member.Name, out string? value) ? value : string.Empty;
            string category = GetCategoryName(member.Name);

            if (member is FieldInfo field && descriptorName == MapTilesDescriptorName)
            {
                for (int mapTileIndex = 0; mapTileIndex < 4; mapTileIndex++)
                {
                    descriptors.Add(new FormattedMemberDescriptor(
                        this,
                        field,
                        category,
                        $"{field.Name}[{mapTileIndex}]",
                        MapTilesDescriptorName,
                        mapTileIndex));
                }
            }
            else
            {
                descriptors.Add(new FormattedMemberDescriptor(
                    this,
                    member,
                    category,
                    member.Name,
                    descriptorName,
                    null));
            }
        }

        return new PropertyDescriptorCollection(descriptors.Cast<PropertyDescriptor>().ToArray(), true);
    }

    private IEnumerable<MemberInfo> GetSortedMembers()
    {
        List<MemberInfo> members = new();

        members.AddRange(_instance.GetType()
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(property => property.CanRead && property.GetIndexParameters().Length == 0));
        members.AddRange(_instance.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance));

        return members
            .OrderBy(member => GetCategorySortIndex(GetCategoryName(member.Name)))
            .ThenBy(member => member.Name, StringComparer.OrdinalIgnoreCase);
    }

    private string GetCategoryName(string memberName)
    {
        if (_categories.TryGetValue(memberName, out string? category) && !string.IsNullOrWhiteSpace(category))
        {
            return category;
        }

        return "Misc";
    }

    private int GetCategorySortIndex(string category)
    {
        return _categoryOrder.TryGetValue(category, out int sortIndex) ? sortIndex : int.MaxValue;
    }

    private static Dictionary<string, int> CreateCategoryOrder(IReadOnlyDictionary<string, string> categories)
    {
        Dictionary<string, int> result = new(StringComparer.Ordinal);
        int index = 0;
        foreach (string category in categories.Values)
        {
            if (!string.IsNullOrWhiteSpace(category) && !result.ContainsKey(category))
            {
                result.Add(category, index++);
            }
        }

        return result;
    }

    private sealed class FormattedMemberDescriptor : PropertyDescriptor
    {
        private static readonly Dictionary<(Type Type, string MemberName), MemberInfo?> MapTileMemberCache = new();
        private static readonly object MapTileMemberCacheLock = new();

        private readonly DebugPropertyGridSelectedObject _owner;
        private readonly MemberInfo _member;
        private readonly string _category;
        private readonly string _displayName;
        private readonly string _descriptorName;
        private readonly int? _mapTileIndex;
        private readonly bool _isReadOnly;
        private readonly bool _useFormattedStringValue;
        private readonly Type _propertyType;

        public FormattedMemberDescriptor(
            DebugPropertyGridSelectedObject owner,
            MemberInfo member,
            string category,
            string displayName,
            string descriptorName,
            int? mapTileIndex)
            : base(displayName, CreateAttributes(category, displayName, !CanEditMember(member, descriptorName, mapTileIndex)))
        {
            _owner = owner;
            _member = member;
            _category = category;
            _displayName = displayName;
            _descriptorName = descriptorName;
            _mapTileIndex = mapTileIndex;
            _isReadOnly = !CanEditMember(member, descriptorName, mapTileIndex);
            _useFormattedStringValue = _isReadOnly;
            _propertyType = _isReadOnly ? typeof(string) : GetEditableMemberType(member);
        }

        public override string Category => _category;
        public override string DisplayName => _displayName;
        public override Type ComponentType => _owner._instance.GetType();
        public override bool IsReadOnly => _isReadOnly;
        public override Type PropertyType => _propertyType;

        public override bool CanResetValue(object component) => false;

        public override object GetValue(object? component)
        {
            object? rawValue = ReadMemberValue();
            if (!_useFormattedStringValue)
            {
                return rawValue ?? GetDefaultValue();
            }

            if (_descriptorName == MapTilesDescriptorName)
            {
                return FormatMapTile(rawValue);
            }

            if (_descriptorName == ShiftedDescriptorName && rawValue is int rawIntValue)
            {
                return rawIntValue + " (" + (rawIntValue >> 16) + ")";
            }

            return Convert.ToString(rawValue, CultureInfo.InvariantCulture) ?? string.Empty;
        }

        public override void ResetValue(object component)
        {
        }

        public override void SetValue(object? component, object? value)
        {
            if (_isReadOnly)
            {
                return;
            }

            switch (_member)
            {
                case PropertyInfo property:
                    property.SetValue(_owner._instance, value);
                    break;
                case FieldInfo field:
                    field.SetValue(_owner._instance, value);
                    break;
            }

            OnValueChanged(component ?? _owner, EventArgs.Empty);
        }

        public override bool ShouldSerializeValue(object component) => false;

        private static Attribute[] CreateAttributes(string category, string displayName, bool isReadOnly)
            => [new ReadOnlyAttribute(isReadOnly), new CategoryAttribute(category), new DisplayNameAttribute(displayName)];

        private static bool CanEditMember(MemberInfo member, string descriptorName, int? mapTileIndex)
        {
            if (mapTileIndex.HasValue || descriptorName == ShiftedDescriptorName || descriptorName == MapTilesDescriptorName)
            {
                return false;
            }

            return member switch
            {
                PropertyInfo property => property.CanRead
                    && property.CanWrite
                    && property.SetMethod?.IsPublic == true
                    && property.GetIndexParameters().Length == 0
                    && IsEditablePrimitiveType(property.PropertyType),
                FieldInfo field => !field.IsInitOnly
                    && !field.IsLiteral
                    && IsEditablePrimitiveType(field.FieldType),
                _ => false,
            };
        }

        private static Type GetEditableMemberType(MemberInfo member)
            => member switch
            {
                PropertyInfo property => property.PropertyType,
                FieldInfo field => field.FieldType,
                _ => typeof(string),
            };

        private static bool IsEditablePrimitiveType(Type type)
        {
            Type actualType = Nullable.GetUnderlyingType(type) ?? type;
            return actualType == typeof(bool)
                || actualType == typeof(byte)
                || actualType == typeof(sbyte)
                || actualType == typeof(short)
                || actualType == typeof(ushort)
                || actualType == typeof(int)
                || actualType == typeof(uint)
                || actualType == typeof(long)
                || actualType == typeof(ulong)
                || actualType == typeof(float)
                || actualType == typeof(double)
                || actualType == typeof(string);
        }

        private object GetDefaultValue()
        {
            Type actualType = Nullable.GetUnderlyingType(_propertyType) ?? _propertyType;
            return actualType.IsValueType ? Activator.CreateInstance(actualType) ?? string.Empty : string.Empty;
        }

        private object? ReadMemberValue()
        {
            try
            {
                return _member switch
                {
                    PropertyInfo property => property.GetValue(_owner._instance),
                    FieldInfo field when _mapTileIndex.HasValue && field.GetValue(_owner._instance) is Array values && _mapTileIndex.Value < values.Length => values.GetValue(_mapTileIndex.Value),
                    FieldInfo field => field.GetValue(_owner._instance),
                    _ => null,
                };
            }
            catch (TargetInvocationException exception)
            {
                return exception.InnerException?.Message ?? exception.Message;
            }
        }

        private static string FormatMapTile(object? mapTile)
        {
            if (mapTile == null)
            {
                return string.Empty;
            }

            return $"{ReadMapTileMember(mapTile, "TileX")}x{ReadMapTileMember(mapTile, "TileY")} {ReadMapTileMember(mapTile, "Walkability")} {ReadMapTileMember(mapTile, "GroundProperty")} {ReadMapTileMember(mapTile, "Slope")} {ReadMapTileMember(mapTile, "Height")} {ReadMapTileMember(mapTile, "TileId")} {ReadMapTileMember(mapTile, "WallTilesOffset")}";
        }

        private static object ReadMapTileMember(object target, string memberName)
        {
            MemberInfo? member = GetCachedMapTileMember(target.GetType(), memberName);
            return member switch
            {
                PropertyInfo property => property.GetValue(target) ?? string.Empty,
                FieldInfo field => field.GetValue(target) ?? string.Empty,
                _ => string.Empty,
            };
        }

        private static MemberInfo? GetCachedMapTileMember(Type targetType, string memberName)
        {
            (Type Type, string MemberName) cacheKey = (targetType, memberName);
            lock (MapTileMemberCacheLock)
            {
                if (MapTileMemberCache.TryGetValue(cacheKey, out MemberInfo? cachedMember))
                {
                    return cachedMember;
                }

                MemberInfo? resolvedMember = targetType.GetProperty(memberName, BindingFlags.Public | BindingFlags.Instance)
                    ?? (MemberInfo?)targetType.GetField(memberName, BindingFlags.Public | BindingFlags.Instance);
                MapTileMemberCache[cacheKey] = resolvedMember;
                return resolvedMember;
            }
        }
    }
}