/***
*
*	Copyright (c) 1996-2001, Valve LLC. All rights reserved.
*	
*	This product contains software technology licensed from Id 
*	Software, Inc. ("Id Technology").  Id Technology (c) 1996 Id Software, Inc. 
*	All Rights Reserved.
*
*   This source code contains proprietary and confidential information of
*   Valve LLC and its suppliers.  Access to this code is restricted to
*   persons who have executed a written SDK license with Valve.  Any access,
*   use or distribution of this code by or to any unlicensed person is illegal.
*
****/

using System;
using System.Globalization;

namespace SharpLife.CommandSystem.TypeProxies
{
    public class BoolTypeProxy : BaseTypeProxy<bool>
    {
        public override bool TryParse(string value, IFormatProvider provider, out bool result) => bool.TryParse(value, out result);
    }

    public class CharTypeProxy : BaseTypeProxy<char>
    {
        public override bool TryParse(string value, IFormatProvider provider, out char result) => char.TryParse(value, out result);
    }

    public class Int8TypeProxy : BaseFormattableTypeProxy<sbyte>
    {
        public override bool TryParse(string value, IFormatProvider provider, out sbyte result) => sbyte.TryParse(value, NumberStyles.Integer, provider, out result);
    }

    public class UInt8TypeProxy : BaseFormattableTypeProxy<byte>
    {
        public override bool TryParse(string value, IFormatProvider provider, out byte result) => byte.TryParse(value, NumberStyles.Integer, provider, out result);
    }

    public class Int16TypeProxy : BaseFormattableTypeProxy<short>
    {
        public override bool TryParse(string value, IFormatProvider provider, out short result) => short.TryParse(value, NumberStyles.Integer, provider, out result);
    }

    public class UInt16TypeProxy : BaseFormattableTypeProxy<ushort>
    {
        public override bool TryParse(string value, IFormatProvider provider, out ushort result) => ushort.TryParse(value, NumberStyles.Integer, provider, out result);
    }

    public class Int32TypeProxy : BaseFormattableTypeProxy<int>
    {
        public override bool TryParse(string value, IFormatProvider provider, out int result) => int.TryParse(value, NumberStyles.Integer, provider, out result);
    }

    public class UInt32TypeProxy : BaseFormattableTypeProxy<uint>
    {
        public override bool TryParse(string value, IFormatProvider provider, out uint result) => uint.TryParse(value, NumberStyles.Integer, provider, out result);
    }

    public class Int64TypeProxy : BaseFormattableTypeProxy<long>
    {
        public override bool TryParse(string value, IFormatProvider provider, out long result) => long.TryParse(value, NumberStyles.Integer, provider, out result);
    }

    public class UInt64TypeProxy : BaseFormattableTypeProxy<ulong>
    {
        public override bool TryParse(string value, IFormatProvider provider, out ulong result) => ulong.TryParse(value, NumberStyles.Integer, provider, out result);
    }

    public class SingleTypeProxy : BaseFormattableTypeProxy<float>
    {
        public override bool TryParse(string value, IFormatProvider provider, out float result) => float.TryParse(value, NumberStyles.Float | NumberStyles.AllowThousands, provider, out result);
    }

    public class DoubleTypeProxy : BaseFormattableTypeProxy<double>
    {
        public override bool TryParse(string value, IFormatProvider provider, out double result) => double.TryParse(value, NumberStyles.Float | NumberStyles.AllowThousands, provider, out result);
    }

    public class DecimalTypeProxy : BaseFormattableTypeProxy<decimal>
    {
        public override bool TryParse(string value, IFormatProvider provider, out decimal result) => decimal.TryParse(value, NumberStyles.Number, provider, out result);
    }

    public class StringTypeProxy : BaseTypeProxy<string>
    {
        public override bool TryParse(string value, IFormatProvider provider, out string result)
        {
            result = value;
            return true;
        }
    }

    public class DateTimeTypeProxy : BaseFormattableTypeProxy<DateTime>
    {
        public override bool TryParse(string value, IFormatProvider provider, out DateTime result) => DateTime.TryParse(value, provider, DateTimeStyles.None, out result);
    }

    public class DateTimeOffsetTypeProxy : BaseFormattableTypeProxy<DateTimeOffset>
    {
        public override bool TryParse(string value, IFormatProvider provider, out DateTimeOffset result) => DateTimeOffset.TryParse(value, provider, DateTimeStyles.None, out result);
    }
}
