using System;
using System.Text;
static class IDConvert
{
    static public string ToString(int id) 
    {
        return id.ToString("X8");
    }

    static public int FromString(string id)
    {
        return int.Parse(id, System.Globalization.NumberStyles.HexNumber);
    }

    static public string ToString(int[] ids)
    {
        var sw = new StringBuilder();

        for (int ix = 0; ix < ids.Length; ix++)
        {
            sw.Append(ToString(ids[ix]));
            if (ix < ids.Length - 1)
                sw.Append(' ');
        }

        return sw.ToString();
    }

    static public int[] FromListString(string ids)
    {
        string[] strIds = ids.Split(
            ' ', StringSplitOptions.RemoveEmptyEntries
        );
        int[] intIds = new int[strIds.Length];
        for (int ix = 0; ix < intIds.Length; ix++)
        {
            intIds[ix] = FromString(strIds[ix]);
        }

        return intIds;
    }
}