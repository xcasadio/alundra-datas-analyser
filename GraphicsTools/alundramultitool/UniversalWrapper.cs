using System.ComponentModel;
using System.Reflection;

namespace alundramultitool;

public class UniversalWrapper : ICustomTypeDescriptor
{
    private readonly object _instance;
    private readonly Dictionary<string, string> _categories;

    public UniversalWrapper(object instance, Dictionary<string, string> categories = null)
    {
        _instance = instance;
        _categories = categories ?? new Dictionary<string, string>();
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
                string category = _categories.TryGetValue(prop.Name, out var cat) ? cat : null;
                props.Add(new ReflectionPropertyDescriptor(_instance, prop, category));
            }
        }

        foreach (var field in _instance.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance))
        {
            string category = _categories.TryGetValue(field.Name, out var cat) ? cat : null;
            props.Add(new ReflectionFieldDescriptor(_instance, field, category));
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

    public ReflectionPropertyDescriptor(object instance, PropertyInfo property, string category)
        : base(property.Name, null)
    {
        _instance = instance;
        _property = property;
        _category = category;
    }

    public override string Category => _category ?? base.Category;
    public override Type PropertyType => _property.PropertyType;
    public override void SetValue(object component, object value) => _property.SetValue(_instance, value);
    public override object GetValue(object component) => _property.GetValue(_instance);
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

    public ReflectionFieldDescriptor(object instance, FieldInfo field, string category)
        : base(field.Name, null)
    {
        _instance = instance;
        _field = field;
        _category = category;
    }

    public override string Category => _category ?? base.Category;
    public override Type PropertyType => _field.FieldType;
    public override void SetValue(object component, object value) => _field.SetValue(_instance, value);
    public override object GetValue(object component) => _field.GetValue(_instance);
    public override bool IsReadOnly => _field.IsInitOnly;
    public override Type ComponentType => _instance.GetType();
    public override bool CanResetValue(object component) => false;
    public override void ResetValue(object component) { }
    public override bool ShouldSerializeValue(object component) => true;
}
