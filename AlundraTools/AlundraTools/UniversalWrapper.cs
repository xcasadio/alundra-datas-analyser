using System.ComponentModel;
using System.Reflection;

namespace AlundraTools;

public class UniversalWrapper : ICustomTypeDescriptor
{
    private readonly object _instance;
    private readonly Dictionary<string, string> _categories;
    private readonly Dictionary<string, string> _descriptors;

    public UniversalWrapper(object instance, Dictionary<string, string> categories = null, Dictionary<string, string> descriptors = null)
    {
        _instance = instance;
        _categories = categories ?? new Dictionary<string, string>();
        _descriptors = descriptors ?? new Dictionary<string, string>();
    }

    public AttributeCollection GetAttributes() => TypeDescriptor.GetAttributes(_instance);
    public string GetClassName() => TypeDescriptor.GetClassName(_instance);
    public string GetComponentName() => TypeDescriptor.GetComponentName(_instance);
    public TypeConverter GetConverter() => TypeDescriptor.GetConverter(_instance);
    public EventDescriptor GetDefaultEvent() => TypeDescriptor.GetDefaultEvent(_instance);
    public PropertyDescriptor GetDefaultProperty() => TypeDescriptor.GetDefaultProperty(_instance);
    public object GetEditor(Type editorBaseType) => TypeDescriptor.GetEditor(_instance, editorBaseType);
    public EventDescriptorCollection GetEvents() => TypeDescriptor.GetEvents(_instance);
    public EventDescriptorCollection GetEvents(Attribute[] attributes) => TypeDescriptor.GetEvents(_instance, attributes);

    public PropertyDescriptorCollection GetProperties() => GetProperties(null);

    public PropertyDescriptorCollection GetProperties(Attribute[] attributes)
    {
        var props = new List<PropertyDescriptor>();

        foreach (var prop in _instance.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (prop.CanRead)
            {
                string category = _categories.GetValueOrDefault(prop.Name);
                var isShifted = _descriptors.GetValueOrDefault(prop.Name) != null;
                props.Add(new ReflectionPropertyDescriptor(_instance, prop, category, isShifted));
            }
        }

        foreach (var field in _instance.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance))
        {
            string category = _categories.GetValueOrDefault(field.Name);
            var isShifted = _descriptors.GetValueOrDefault(field.Name) != null;
            props.Add(new ReflectionFieldDescriptor(_instance, field, category, isShifted));
        }

        return new PropertyDescriptorCollection(props.ToArray());
    }

    public object GetPropertyOwner(PropertyDescriptor pd) => _instance;
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
    private readonly object _instance;
    private readonly FieldInfo _field;
    private readonly string _category;
    private readonly bool _isShifted;

    public ReflectionFieldDescriptor(object instance, FieldInfo field, string category, bool isShifted = false)
        : base(field.Name, null)
    {
        _instance = instance;
        _field = field;
        _category = category;
        _isShifted = isShifted;
    }

    public override object GetValue(object component)
    {
        if (!_isShifted)
        {
            return _field.GetValue(_instance);
        }

        int rawValue = (int)_field.GetValue(_instance);
        int shiftedValue = rawValue >> 16;
        return rawValue + " (" + shiftedValue + ")";
    }

    public override void SetValue(object component, object value)
    {
        if (!_isShifted)
        {
            _field.SetValue(_instance, value);
            return;
        }

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

    public override string Category => _category ?? base.Category;
    public override Type PropertyType => _field.FieldType;
    public override bool IsReadOnly => _field.IsInitOnly;
    public override Type ComponentType => _instance.GetType();
    public override bool CanResetValue(object component) => false;
    public override void ResetValue(object component) { }
    public override bool ShouldSerializeValue(object component) => true;
}
