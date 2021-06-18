using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public struct IntVector3 : IEquatable<IntVector3>
{
	public int x;
	public int y;
	public int z;

	public IntVector3(int x, int y, int z)
	{
		this.x = x;
		this.y = y;
		this.z = z;
	}

	public IntVector3(Vector3 v)
	{
		x = Mathf.RoundToInt(v.x);
		y = Mathf.RoundToInt(v.y);
		z = Mathf.RoundToInt(v.z);
	}

	public static implicit operator Vector3(IntVector3 c)
	{
		return new Vector3(c.x, c.y, c.z);
	}

	public static implicit operator IntVector3(Vector3 v)
	{
		return new IntVector3(v);
	}

	public int this[int i]
	{
		get { switch (i) { case 0: return x; case 1: return y; case 2: return z; default: return int.MinValue; } }
		set { switch (i) { case 0: x = value; break; case 1: y = value; break; case 2: z = value; break; } }
	}

	public static bool operator ==(IntVector3 a, IntVector3 b)
	{
		return a.x == b.x && a.y == b.y && a.z == b.z;
	}

	public static bool operator !=(IntVector3 a, IntVector3 b)
	{
		return a.x != b.x || a.y != b.y || a.z != b.z;
	}

	public static IntVector3 operator +(IntVector3 c, IntVector3 d)
	{
		return new IntVector3 { x = c.x + d.x, y = c.y + d.y, z = c.z + d.z };
	}

	public static IntVector3 operator -(IntVector3 c, IntVector3 d)
	{
		return new IntVector3 { x = c.x - d.x, y = c.y - d.y, z = c.z - d.z };
	}

	public static IntVector3 operator -(IntVector3 a)
	{
		return new IntVector3 { x = -a.x, y = -a.y, z = -a.z };
	}
	
	public static Vector3 operator +(IntVector3 iv, Vector3 fv)
	{
		return new Vector3 { x = iv.x + fv.x, y = iv.y + fv.y, z = iv.z + fv.z };
	}

	public static Vector3 operator +(Vector3 fv, IntVector3 iv)
	{
		return new Vector3 { x = iv.x + fv.x, y = iv.y + fv.y, z = iv.z + fv.z };
	}

	public static Vector3 operator -(IntVector3 iv, Vector3 fv)
	{
		return new Vector3 { x = iv.x - fv.x, y = iv.y - fv.y, z = iv.z - fv.z };
	}

	public static Vector3 operator -(Vector3 fv, IntVector3 iv)
	{
		return new Vector3 { x = fv.x - iv.x, y = fv.y - iv.y, z = fv.z - iv.z };
	}
	
	public static IntVector3 operator *(IntVector3 a, int b)
	{
		return new IntVector3 { x = a.x * b, y = a.y * b, z = a.z * b };
	}

	public static IntVector3 operator /(IntVector3 a, int b)
	{
		return new IntVector3 { x = a.x / b, y = a.y / b, z = a.z / b };
	}

	public static IntVector3 Max(IntVector3 a, IntVector3 b)
	{
		return new IntVector3 { x = a.x > b.x ? a.x : b.x, y = a.y > b.y ? a.y : b.y, z = a.z > b.z ? a.z : b.z };
	}

	public static IntVector3 Min(IntVector3 a, IntVector3 b)
	{
		return new IntVector3 { x = a.x < b.x ? a.x : b.x, y = a.y < b.y ? a.y : b.y, z = a.z < b.z ? a.z : b.z };
	}

	// Parses strings of the form "x,y,z", which is NOT what ToString outputs. Make sure strings with [] and/or spaces in them go through ConvertFromString
	public static IntVector3 Parse(string s)
	{
		if (s != null)
		{
			string[] parts = s.Split(new char[] { ',' });
			if (parts.Length == 3)
			{
				return new IntVector3 { x = int.Parse(parts[0]), y = int.Parse(parts[1]), z = int.Parse(parts[2]) };
			}
			else
			{
				throw new ArgumentException(String.Format("IntVector3.Parse - Input string not in expected format 'x,y,z' (got '{0}'", s));
			}
		}

		return IntVector3.invalid;
	}

	// helpers specific for tank AP coordinates:
	//   tank-local integer attach points co-ordinates, doubled relative to local coord space,
	//   so coords are exact integers e.g. (-1, 3, 1) instead of (-0.5, 1.5, 0.5)

	public Vector3 APtoLocal()
	{
		return new Vector3((float)x * 0.5f, (float)y * 0.5f, (float)z * 0.5f);
	}

	public Vector3 APtoWorld(Transform t)
	{
		return t.TransformPoint(APtoLocal());
	}

	public bool APFaceX()
	{
		// half coordinate indicates a left or right face
		return (x & 1) == 1;
	}

	public bool APFaceY()
	{
		// half coordinate indicates a top or bottom face
		return (y & 1) == 1;
	}

	public bool APFaceZ()
	{
		// half coordinate indicates a front or back face
		return (z & 1) == 1;
	}

	public IntVector3 PadHalf()
	{
		// for each dimension:
		// (x + Sign(x) * IsOdd(x)) / 2
		// ie if x is odd, make one bigger, then divide by 2
		return new IntVector3(	(x + (1 | (x >> 31)) * (x & 1)) >> 1,
								(y + (1 | (y >> 31)) * (y & 1)) >> 1,
								(z + (1 | (z >> 31)) * (z & 1)) >> 1);
	}

	public IntVector3 PadHalfDown()
	{
		// for each dimension:
		// (x - Sign(x) * IsOdd(x)) / 2
		// ie if x is odd, make one *smaller* then divide by 2
		return new IntVector3(	(x - (1 | (x >> 31)) * (x & 1)) >> 1,
								(y - (1 | (y >> 31)) * (y & 1)) >> 1,
								(z - (1 | (z >> 31)) * (z & 1)) >> 1);
	}

	public IntVector3 AxisUnit()
	{
		// unit vector indicating the main axis of the AP (the one with 'half cell' length)
		return new IntVector3(	(1 | (x >> 31)) * (x & 1),
								(1 | (y >> 31)) * (y & 1),
								(1 | (z >> 31)) * (z & 1));
	}

	public byte APHalfBits()
	{
		// get a bitfield with one bit for each half coordinate, using the pattern __XXYYZZ,
		// where the first bit for each dimension is used for a 'down' value and the second for an 'up'
		// (only one bit should be set for any genuine AP coord)
		return (byte)(
			(((x & 1) << (1 & (x >> 31))) << 4)
		|	(((y & 1) << (1 & (y >> 31))) << 2)
		|	 ((z & 1) << (1 & (z >> 31)))
			);
	}

	public int sqrMagnitude
	{
		get
		{
			return x * x + y * y + z * z;
		}
	}

	public float magnitude
	{
		get
		{
			return Mathf.Sqrt(sqrMagnitude);
		}
	}

	// Equals and GetHashCode methods allow this to be used as a Dictionary key

	public override bool Equals(object obj)
	{
		return obj is IntVector3 ? this == (IntVector3)obj : false;
	}

	public override int GetHashCode()
	{
		//return x * y * z;
		//return x ^ (y << 2) ^ (z >> 2);

		return ((x & 0xff) << 16) | ((y & 0xff) << 8) | (z & 0xff); // unique hash up to +/-128
	}

	public bool Equals(IntVector3 other)
	{
		return x == other.x && y == other.y && z == other.z;
	}

	public override string ToString()
	{
		if (this == invalid)
			return "[invalid]";
		else
			return string.Format("[{0}, {1}, {2}]", x, y, z);
	}

	// Unlike the more lightweight Parse method, this will accept strings with spaces and [], as outputted by ToString
	public static IntVector3 ConvertFromString(string _string)
	{
		if (_string[0] != '[' || _string[_string.Length - 1] != ']')
		{
			throw new ArgumentException(String.Format("IntVector3.ConvertFromString - Input string not in expected format '[x, y, z]' (got '{0}'", _string));
		}

		if (_string.Equals("[invalid]"))
			return IntVector3.invalid;

		string trimmed = _string.Trim(new char[] { '[', ' ', ']' });
		trimmed = trimmed.Replace(" ", "");

		// Expecting 4 character trim, [] and two spaces
		Debug.Assert(_string.Length == trimmed.Length + 4, "IntVector3.ConvertFromString - Trimmed more characters from string than expected: \"" + _string + "\" became \"" + trimmed + "\"");

		// Pass a string in the format "x,y,z" to Parse
		return Parse(trimmed);
	}

	public static IntVector3 zero;
	public static IntVector3 one;
	public static IntVector3 forward;
	public static IntVector3 right;
	public static IntVector3 up;
	public static IntVector3 invalid;

	static IntVector3()
	{
		zero = new IntVector3 { x = 0, y = 0, z = 0 };
		one = new IntVector3 { x = 1, y = 1, z = 1 };
		forward = new IntVector3 { x = 0, y = 0, z = 1 };
		right = new IntVector3 { x = 1, y = 0, z = 0 };
		up = new IntVector3 { x = 0, y = 1, z = 0 };
		invalid = new IntVector3 { x = int.MaxValue, y = int.MaxValue, z = int.MaxValue };
	}
}

[System.Serializable]
public struct IntVector2 : IEquatable<IntVector2>
{
	public int x;
	public int y;

	public IntVector2(int x, int y)
	{
		this.x = x;
		this.y = y;
	}

	public IntVector2(Vector2 v)
	{
		x = Mathf.RoundToInt(v.x);
		y = Mathf.RoundToInt(v.y);
	}

	public static implicit operator Vector2(IntVector2 c)
	{
		return new Vector2(c.x, c.y);
	}

	public static implicit operator IntVector2(Vector2 v)
	{
		return new IntVector2(v);
	}

	public int this[int i]
	{
		get { switch (i) { case 0: return x; case 1: return y; default: return int.MinValue; } }
		set { switch (i) { case 0: x = value; break; case 1: y = value; break; } }
	}

	public static bool operator ==(IntVector2 a, IntVector2 b)
	{
		return a.x == b.x && a.y == b.y;
	}

	public static bool operator !=(IntVector2 a, IntVector2 b)
	{
		return a.x != b.x || a.y != b.y;
	}

	public static IntVector2 operator +(IntVector2 c, IntVector2 d)
	{
		return new IntVector2 { x = c.x + d.x, y = c.y + d.y };
	}

	public static IntVector2 operator -(IntVector2 c, IntVector2 d)
	{
		return new IntVector2 { x = c.x - d.x, y = c.y - d.y };
	}

	public static IntVector2 operator -(IntVector2 a)
	{
		return new IntVector2 { x = -a.x, y = -a.y };
	}

	public static IntVector2 operator *(IntVector2 a, int b)
	{
		return new IntVector2 { x = a.x * b, y = a.y * b };
	}

	public static IntVector2 operator /(IntVector2 a, int b)
	{
		return new IntVector2 { x = a.x / b, y = a.y / b };
	}

	public static IntVector2 Max(IntVector2 a, IntVector2 b)
	{
		return new IntVector2 { x = a.x > b.x ? a.x : b.x, y = a.y > b.y ? a.y : b.y };
	}

	public static IntVector2 Min(IntVector2 a, IntVector2 b)
	{
		return new IntVector2 { x = a.x < b.x ? a.x : b.x, y = a.y < b.y ? a.y : b.y };
	}

	// Parses strings of the form "x,y", which is NOT what ToString outputs. Make sure strings with [] and/or spaces in them go through ConvertFromString
	public static IntVector2 Parse(string s)
	{
		if (s != null)
		{
			string[] parts = s.Split(new char[] { ',' });
			if (parts.Length == 2)
			{
				return new IntVector2 { x = int.Parse(parts[0]), y = int.Parse(parts[1]) };
			}
			else
			{
				throw new ArgumentException(String.Format("IntVector2.Parse - Input string not in expected format 'x,y' (got '{0}'", s));
			}
		}

		return IntVector2.invalid;
	}

	// Equals and GetHashCode methods allow this to be used as a Dictionary key

	public override bool Equals(object obj)
	{
		return obj is IntVector2 ? this == (IntVector2)obj : false;
	}

	public override int GetHashCode()
	{
		return (x << 16) | y; // unique hash up to +/-32768
	}

	public bool Equals(IntVector2 other)
	{
		return x == other.x && y == other.y;
	}

	public override string ToString()
	{
		if (this == invalid)
			return "[invalid]";
		else
			return string.Format("[{0}, {1}]", x, y);
	}

	// Unlike the more lightweight Parse method, this will accept strings with spaces and [], as outputted by ToString
	public static IntVector2 ConvertFromString(string _string)
	{
		if (_string[0] != '[' || _string[_string.Length - 1] != ']')
		{
			throw new ArgumentException(String.Format("IntVector2.ConvertFromString - Input string not in expected format '[x, y]' (got '{0}'", _string));
		}

		if (_string.Equals("[invalid]")) 
			return IntVector2.invalid;

		string trimmed = _string.Trim(new char[] { '[', ' ', ']' });
		trimmed = trimmed.Replace(" ", "");

		// Expecting 3 character trim, [] and a space
		Debug.Assert(_string.Length == trimmed.Length + 3, "IntVector2.ConvertFromString - Trimmed more characters from string than expected: \"" + _string + "\" became \"" + trimmed + "\"");

		// Pass a string in the format "x,y" to Parse
		return Parse(trimmed);
	}

	public static IntVector2 zero;
	public static IntVector2 one;
	public static IntVector2 invalid;

	static IntVector2()
	{
		zero = new IntVector2 { x = 0, y = 0 };
		one = new IntVector2 { x = 1, y = 1 };
		invalid = new IntVector2 { x = int.MaxValue, y = int.MaxValue };
	}
}

public static class IntVectorExtensions
{
	// helpers specific for tank AP coordinates

	public static IntVector3 LocalToAP(this Vector3 v)
	{
		return new IntVector3 { x = Mathf.RoundToInt(v.x + v.x), y = Mathf.RoundToInt(v.y + v.y), z = Mathf.RoundToInt(v.z + v.z) };
	}

	public static IntVector3 WorldToAP(this Vector3 v, Transform t)
	{
		Debug.Assert(false, "Vector3.WorldToAP is SLOW, don't use it please!");

		return t.InverseTransformPoint(v).LocalToAP();
	}
}

[System.Serializable]
public struct IntBounds
{
	IntVector3 _min;
	IntVector3 _max;

	public IntVector3 min { get { return _min; } set { _min = value; } }
	public IntVector3 max { get { return _max; } set { _max = value; } }
	public IntVector3 size { get { return _max - _min; } }

	public IntBounds(IntVector3 min, IntVector3 max)
	{
		this._min = min;
		this._max = max;
	}

	public IntBounds(Bounds bounds)
	{
		this._min = bounds.min;
		this._max = bounds.max;
	}

	public static implicit operator Bounds(IntBounds b)
	{
		Bounds bounds = new Bounds();
		bounds.SetMinMax(b._min, b._max);
		return bounds;
	}

	public void Set(IntVector3 min, IntVector3 max)
	{
		this._min = min;
		this._max = max;
	}

	public IntBounds Translate(IntVector3 offset)
	{
		return new IntBounds(_min + offset, _max + offset);
	}

	public IntBounds Union(IntBounds other)
	{
		return new IntBounds(IntVector3.Min(_min, other._min), IntVector3.Max(_max, other._max));
	}

	public IntBounds Intersection(IntBounds other)
	{
		return new IntBounds(IntVector3.Max(_min, other._min), IntVector3.Min(_max, other._max));
	}

	public IntBounds Clamp(int lower, int upper)
	{
		return new IntBounds(IntVector3.Max(_min, new IntVector3(lower, lower, lower)), IntVector3.Min(_max, new IntVector3(upper, upper, upper)));
	}

	public static bool operator ==(IntBounds a, IntBounds b)
	{
		return a._min == b._min && a._max == b._max;
	}

	public static bool operator !=(IntBounds a, IntBounds b)
	{
		return a._min != b._min || a._max != b._max;
	}

	public override bool Equals(object other)
	{
		return (IntBounds)other == this;
	}

	public override int GetHashCode()
	{
		// ...or comment the assert, if this shocking non-implementation is actually considered adequate  :O==
		Debug.Assert(false, "please replace this implementation, if GetHashCode() is actually needed to work");
		return (_min.GetHashCode() << 8) | _max.GetHashCode();
	}

	public override string ToString()
	{
		return string.Format("{{{0} - {1}}}", _min, _max);
	}
}