using System;
using System.Runtime.InteropServices;

public struct Protector<T> where T : struct
{
    T _value;
    byte[] _key;

    public static implicit operator Protector<T>(T v) => new Protector<T>(v);
    public static implicit operator T(Protector<T> v) => v.Value;

    public override bool Equals(object obj) => obj is Protector<T> && Value.Equals(((Protector<T>)obj).Value);
    public override int GetHashCode() => Value.GetHashCode();
    public override string ToString() => Value.ToString();

    public Protector(T val)
    {
        _value = val;
        _key = CreateKey();
        Xor(ref _value);
    }

    public T Value
    {
        get
        {
            InitCheck();
            var val = _value;
            Xor(ref val);
            return val;
        }
        set
        {
            InitCheck();
            var work = value;
            Xor(ref work);
            _value = work;
        }
    }

    void InitCheck()
    {
        if (_key is null)
        {
            _value = default;
            _key = CreateKey();
            Xor(ref _value);
        }
    }

    static byte[] CreateKey()
    {
        var key = new byte[Marshal.SizeOf(typeof(T))];
        new System.Random().NextBytes(key);
        return key;
    }

    void Xor(ref T val)
    {
        var bytes = MemoryMarshal.AsBytes(MemoryMarshal.CreateSpan(ref val, 1));
        var keys = new ReadOnlySpan<byte>(_key);
        for (int i = 0; i < bytes.Length; i++)
        {
            bytes[i] ^= keys[i];
        }
    }
}