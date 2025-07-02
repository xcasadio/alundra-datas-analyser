using System.Collections;
using System.Reflection;
using AlundraEngine.Editor;
using OfficeOpenXml;

public static class FrameSnapshotComparer
{
    // Liste de FrameSnapshot pour original, décompilé
    public static void ExportComparisonToExcel(
        List<FrameSnapshot> originalFrames,
        List<FrameSnapshot> decompiledFrames,
        string filePath)
    {
        ExcelPackage.License.SetNonCommercialPersonal("xcasadio");
        using var package = new ExcelPackage();

        // ---- Entités : détection du nombre d’entités max de toutes les frames
        int entityCount = 18;
        //foreach (var f in originalFrames)
        //    entityCount = System.Math.Max(entityCount, f.Entities?.Length ?? 0);
        //foreach (var f in decompiledFrames)
        //    entityCount = System.Math.Max(entityCount, f.Entities?.Length ?? 0);

        var entityColumns = EntityComparer.BuildColumns();

        // ---- Feuilles pour chaque entité ----
        for (int i = 0; i < entityCount; i++)
        {
            var wsOri = package.Workbook.Worksheets.Add($"entity{i}_ori");
            var wsDec = package.Workbook.Worksheets.Add($"entity{i}_decom");
            var wsDiff = package.Workbook.Worksheets.Add($"entity{i}_diff");

            // En-tête
            for (int c = 0; c < entityColumns.Count; c++)
            {
                wsOri.Cells[1, c + 1].Value = entityColumns[c].Label;
                wsDec.Cells[1, c + 1].Value = entityColumns[c].Label;
                wsDiff.Cells[1, c + 1].Value = entityColumns[c].Label;
            }

            int nFrames = Math.Min(originalFrames.Count, decompiledFrames.Count);
            for (int f = 0; f < nFrames; f++)
            {
                var entOri = (originalFrames[f].Entities != null && i < originalFrames[f].Entities.Length)
                    ? originalFrames[f].Entities[i] : null;
                var entDec = (decompiledFrames[f].Entities != null && i < decompiledFrames[f].Entities.Length)
                    ? decompiledFrames[f].Entities[i] : null;

                for (int c = 0; c < entityColumns.Count; c++)
                {
                    wsOri.Cells[f + 2, c + 1].Value = entOri != null ? entityColumns[c].GetValue(entOri) : 0;
                    wsDec.Cells[f + 2, c + 1].Value = entDec != null ? entityColumns[c].GetValue(entDec) : 0;
                    double v1 = entOri != null ? entityColumns[c].GetValue(entOri) : 0;
                    double v2 = entDec != null ? entityColumns[c].GetValue(entDec) : 0;
                    wsDiff.Cells[f + 2, c + 1].Value = v1 - v2;
                }
            }
        }

        // ---- Feuilles "globals" ----
        var globalColumns = BuildGlobalColumns();

        var wsGlobOri = package.Workbook.Worksheets.Add("globals_ori");
        var wsGlobDec = package.Workbook.Worksheets.Add("globals_decom");
        var wsGlobDiff = package.Workbook.Worksheets.Add("globals_diff");
        for (int c = 0; c < globalColumns.Count; c++)
        {
            wsGlobOri.Cells[1, c + 1].Value = globalColumns[c].Label;
            wsGlobDec.Cells[1, c + 1].Value = globalColumns[c].Label;
            wsGlobDiff.Cells[1, c + 1].Value = globalColumns[c].Label;
        }
        int nGlobFrames = Math.Min(originalFrames.Count, decompiledFrames.Count);
        for (int f = 0; f < nGlobFrames; f++)
        {
            for (int c = 0; c < globalColumns.Count; c++)
            {
                double v1 = globalColumns[c].GetValue(originalFrames[f]);
                double v2 = globalColumns[c].GetValue(decompiledFrames[f]);
                wsGlobOri.Cells[f + 2, c + 1].Value = v1;
                wsGlobDec.Cells[f + 2, c + 1].Value = v2;
                wsGlobDiff.Cells[f + 2, c + 1].Value = v1 - v2;
            }
        }

        package.SaveAs(new FileInfo(filePath));
    }

    // Récupère tous les champs/propriétés numériques simples et tableaux simples de FrameSnapshot (hors Entities)
    public static List<EntityComparer.ColumnInfo> BuildGlobalColumns()
    {
        var columns = new List<EntityComparer.ColumnInfo>();
        var t = typeof(FrameSnapshot);

        foreach (var prop in t.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
        {
            if (prop.Name == "Entities") continue;
            var propType = prop.PropertyType;
            if (ObjectComparer.IsSimpleNumericType(propType))
            {
                string name = prop.Name;
                columns.Add(new EntityComparer.ColumnInfo
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
                    columns.Add(new EntityComparer.ColumnInfo
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
        foreach (var field in t.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
        {
            if (field.Name == "Entities") continue;
            var fieldType = field.FieldType;
            if (ObjectComparer.IsSimpleNumericType(fieldType))
            {
                string name = field.Name;
                columns.Add(new EntityComparer.ColumnInfo
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
                    columns.Add(new EntityComparer.ColumnInfo
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