using System.ComponentModel;
using System.Reflection;
using AlundraEngine.DatasBin;

namespace AlundraTools;

public class UniversalWrapper : ICustomTypeDescriptor
{
    public object Instance { get; }
    private readonly Dictionary<string, string> _categories;
    private readonly Dictionary<string, string> _descriptors;

    public UniversalWrapper(object instance, Dictionary<string, string> categories = null, Dictionary<string, string> descriptors = null)
    {
        Instance = instance;
        _categories = categories ?? new Dictionary<string, string>();
        _descriptors = descriptors ?? new Dictionary<string, string>();
    }

    public AttributeCollection GetAttributes() => TypeDescriptor.GetAttributes(Instance);
    public string GetClassName() => TypeDescriptor.GetClassName(Instance);
    public string GetComponentName() => TypeDescriptor.GetComponentName(Instance);
    public TypeConverter GetConverter() => TypeDescriptor.GetConverter(Instance);
    public EventDescriptor GetDefaultEvent() => TypeDescriptor.GetDefaultEvent(Instance);
    public PropertyDescriptor GetDefaultProperty() => TypeDescriptor.GetDefaultProperty(Instance);
    public object GetEditor(Type editorBaseType) => TypeDescriptor.GetEditor(Instance, editorBaseType);
    public EventDescriptorCollection GetEvents() => TypeDescriptor.GetEvents(Instance);
    public EventDescriptorCollection GetEvents(Attribute[] attributes) => TypeDescriptor.GetEvents(Instance, attributes);

    public PropertyDescriptorCollection GetProperties() => GetProperties(null);

    public PropertyDescriptorCollection GetProperties(Attribute[] attributes)
    {
        var props = new List<PropertyDescriptor>();

        foreach (var prop in Instance.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (prop.CanRead)
            {
                string category = _categories.GetValueOrDefault(prop.Name);
                var isShifted = _descriptors.GetValueOrDefault(prop.Name) != null; 
                props.Add(new ReflectionPropertyDescriptor(Instance, prop, category, isShifted));
            }
        }

        foreach (var field in Instance.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance))
        {
            string category = _categories.GetValueOrDefault(field.Name);
            var descriptorTypeName = _descriptors.GetValueOrDefault(field.Name);

            if (descriptorTypeName == null)
            {
                props.Add(new ReflectionFieldDescriptor(Instance, field, category));
            }
            else if (descriptorTypeName == nameof(ShiftedFieldDescriptor))
            {
                props.Add(new ShiftedFieldDescriptor(Instance, field, category));
            }
            else if (descriptorTypeName == nameof(MapTilesFieldDescriptor))
            {
                props.Add(new MapTilesFieldDescriptor(Instance, field, category, 0));
                props.Add(new MapTilesFieldDescriptor(Instance, field, category, 1));
                props.Add(new MapTilesFieldDescriptor(Instance, field, category, 2));
                props.Add(new MapTilesFieldDescriptor(Instance, field, category, 3));
            }
        }

        return new PropertyDescriptorCollection(props.ToArray());
    }

    public object GetPropertyOwner(PropertyDescriptor pd) => Instance;
}

public class ReflectionPropertyDescriptor : PropertyDescriptor
{
    private readonly object _instance;
    private readonly PropertyInfo _property;
    private readonly string _category;
    private readonly bool _isShifted;

    public ReflectionPropertyDescriptor(object instance, PropertyInfo property, string category, bool isShifted = false)
        : base(property.Name, null)
    {
        _instance = instance;
        _property = property;
        _category = category;
        _isShifted = isShifted;
    }

    public override object GetValue(object component)
    {
        if (!_isShifted)
        {
            return _property.GetValue(_instance);
        }

        int rawValue = (int)_property.GetValue(_instance);
        int shiftedValue = rawValue >> 16;
        return rawValue + " (" + shiftedValue + ")";
    }

    public override void SetValue(object component, object value)
    {
        if (!_isShifted)
        {
            _property.SetValue(_instance, value);
            return;
        }

        if (value is string stringValue)
        {
            if (int.TryParse(stringValue.Split('(')[0].Trim(), out int newValue))
            {
                _property.SetValue(_instance, newValue);
            }
        }
        else
        {
            _property.SetValue(component, value);
        }
    }

    public override string Category => _category ?? base.Category;
    public override Type PropertyType => _property.PropertyType;
    public override bool IsReadOnly => !_property.CanWrite;
    public override Type ComponentType => _instance.GetType();
    public override bool CanResetValue(object component) => false;
    public override void ResetValue(object component) { }
    public override bool ShouldSerializeValue(object component) => true;
}

public class ReflectionFieldDescriptor : PropertyDescriptor
{
    protected readonly object _instance;
    protected readonly FieldInfo _field;
    private readonly string _category;

    public ReflectionFieldDescriptor(object instance, FieldInfo field, string category)
        : base(field.Name, null)
    {
        _instance = instance;
        _field = field;
        _category = category;
    }


    protected ReflectionFieldDescriptor(object instance, FieldInfo field, string name,  string category)
        : base(name, null)
    {
        _instance = instance;
        _field = field;
        _category = category;
    }

    public override object GetValue(object component)
    {
        return _field.GetValue(_instance);
    }

    public override void SetValue(object component, object value)
    {
        _field.SetValue(_instance, value);
    }

    public override string Category => _category ?? base.Category;
    public override Type PropertyType => _field.FieldType;
    public override bool IsReadOnly => _field.IsInitOnly;
    public override Type ComponentType => _instance.GetType();
    public override bool CanResetValue(object component) => false;
    public override void ResetValue(object component) { }
    public override bool ShouldSerializeValue(object component) => true;
}

public class ShiftedFieldDescriptor : ReflectionFieldDescriptor
{
    public ShiftedFieldDescriptor(object instance, FieldInfo field, string category)
        : base(instance, field, category)
    {
    }

    public override object GetValue(object component)
    {
        int rawValue = (int)_field.GetValue(_instance);
        int shiftedValue = rawValue >> 16;
        return rawValue + " (" + shiftedValue + ")";
    }

    public override void SetValue(object component, object value)
    {
        if (value is string stringValue)
        {
            if (int.TryParse(stringValue.Split('(')[0].Trim(), out int newValue))
            {
                _field.SetValue(_instance, newValue);
            }
        }
        else
        {
            _field.SetValue(component, value);
        }
    }
}

public class MapTilesFieldDescriptor : ReflectionFieldDescriptor
{
    private readonly int _index;

    public MapTilesFieldDescriptor(object instance, FieldInfo field, string category, int index)
        : base(instance, field, $"{field.Name}[{index}]", category)
    {
        _index = index;
    }

    public override object GetValue(object component)
    {
        var mapTiles = (MapTile[])_field.GetValue(_instance);
        var mapTile = mapTiles[_index];

        if (mapTile == null)
        {
            return null;
        }

        return $"{mapTile.TileX}x{mapTile.TileY} {mapTile.Walkability} {mapTile.GroundProperty} {mapTile.Slope} {mapTile.Height} {mapTile.TileId} {mapTile.WallTilesOffset}";
    }

    public override void SetValue(object component, object value)
    {
        _field.SetValue(component, value);
    }
}