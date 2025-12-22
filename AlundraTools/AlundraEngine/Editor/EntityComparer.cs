using System.Collections;
using AlundraEngine.Gameplay;
using OfficeOpenXml;
using System.Reflection;


public static class EntityComparer
{
    public static void ExportComparisonToExcel(List<Entity> originalList, List<Entity> decompiledList, string filePath)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        using var package = new ExcelPackage();

        var columns = BuildColumns();
        int rowCount = Math.Min(originalList.Count, decompiledList.Count);

        for (int i = 0; i < rowCount; i++)
        {
            // Feuille Original
            var wsOri = package.Workbook.Worksheets.Add($"entity{i}_ori");
            WriteSingleEntitySheet(wsOri, originalList[i], columns);

            // Feuille Decompiled
            var wsDecom = package.Workbook.Worksheets.Add($"entity{i}_decom");
            WriteSingleEntitySheet(wsDecom, decompiledList[i], columns);

            // Feuille Diff
            var wsDiff = package.Workbook.Worksheets.Add($"entity{i}_diff");
            WriteSingleDiffSheet(wsDiff, originalList[i], decompiledList[i], columns);
        }

        package.SaveAs(new FileInfo(filePath));
    }

    // -- Une feuille pour l'entité seule (original ou decompiled)
    static void WriteSingleEntitySheet(ExcelWorksheet ws, Entity entity, List<ColumnInfo> columns)
    {
        for (int c = 0; c < columns.Count; c++)
            ws.Cells[1, c + 1].Value = columns[c].Label;
        for (int c = 0; c < columns.Count; c++)
            ws.Cells[2, c + 1].Value = columns[c].GetValue(entity);
    }

    // -- Une feuille pour la différence entre deux entités
    static void WriteSingleDiffSheet(ExcelWorksheet ws, Entity entity1, Entity entity2, List<ColumnInfo> columns)
    {
        for (int c = 0; c < columns.Count; c++)
            ws.Cells[1, c + 1].Value = columns[c].Label;
        for (int c = 0; c < columns.Count; c++)
        {
            double v1 = columns[c].GetValue(entity1);
            double v2 = columns[c].GetValue(entity2);
            ws.Cells[2, c + 1].Value = v1 - v2;
        }
    }

    // -- La partie colonne ne change pas par rapport à la version précédente --
    public class ColumnInfo
    {
        public string Label;
        public Func<object, double> GetValue;
    }

    public static List<ColumnInfo> BuildColumns()
    {
        var columns = new List<ColumnInfo>();
        var t = typeof(Entity);

        // Propriétés
        foreach (var prop in t.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
        {
            if (!prop.CanRead) continue;
            var propType = prop.PropertyType;
            if (ObjectComparer.IsSimpleNumericType(propType))
            {
                string name = prop.Name;
                columns.Add(new ColumnInfo
                {
                    Label = name,
                    GetValue = o => Convert.ToDouble(prop.GetValue(o) ?? 0)
                });
            }
            else if (ObjectComparer.IsArrayOfSimpleType(propType, out Type eltType))
            {
                var len = ObjectComparer.GetArrayLength(prop, null);
                for (int i = 0; i < len; i++)
                {
                    int idx = i;
                    string colName = $"{prop.Name}[{i}]";
                    columns.Add(new ColumnInfo
                    {
                        Label = colName,
                        GetValue = o =>
                        {
                            if (prop.GetValue(o) is IList arr && arr.Count > idx)
                                return Convert.ToDouble(arr[idx]);
                            return 0;
                        }
                    });
                }
            }
        }
        // Champs
        foreach (var field in t.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
        {
            var fieldType = field.FieldType;
            if (ObjectComparer.IsSimpleNumericType(fieldType))
            {
                string name = field.Name;
                columns.Add(new ColumnInfo
                {
                    Label = name,
                    GetValue = o => Convert.ToDouble(field.GetValue(o) ?? 0)
                });
            }
            else if (ObjectComparer.IsArrayOfSimpleType(fieldType, out Type eltType))
            {
                var len = ObjectComparer.GetArrayLength(null, field);
                for (int i = 0; i < len; i++)
                {
                    int idx = i;
                    string colName = $"{field.Name}[{i}]";
                    columns.Add(new ColumnInfo
                    {
                        Label = colName,
                        GetValue = o =>
                        {
                            if (field.GetValue(o) is IList arr && arr.Count > idx)
                                return Convert.ToDouble(arr[idx]);
                            return 0;
                        }
                    });
                }
            }
        }
        return columns;
    }

    
}

public static class ObjectComparer
{
    public static bool IsSimpleNumericType(Type type)
    {
        return type == typeof(int) || type == typeof(uint) || type == typeof(short) || type == typeof(ushort)
               || type == typeof(byte) || type == typeof(sbyte)
               || type == typeof(float) || type == typeof(double) || type == typeof(long) || type == typeof(ulong);
    }

    public static bool IsArrayOfSimpleType(Type type, out Type eltType)
    {
        eltType = null;
        if (type.IsArray && type.GetArrayRank() == 1)
        {
            var t = type.GetElementType();
            if (IsSimpleNumericType(t))
            {
                eltType = t;
                return true;
            }
        }
        return false;
    }

    public static int GetArrayLength(PropertyInfo prop, FieldInfo field)
    {
        object arrayObj = null;
        if (prop != null)
        {
            try
            {
                var inst = Activator.CreateInstance(typeof(Entity));
                arrayObj = prop.GetValue(inst);
            }
            catch { }
        }
        if (field != null)
        {
            try
            {
                var inst = Activator.CreateInstance(typeof(Entity));
                arrayObj = field.GetValue(inst);
            }
            catch { }
        }
        if (arrayObj is IList arr)
            return arr.Count;
        return 0;
    }
}