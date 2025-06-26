using System.Linq;
using System.Collections.Generic;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;

public static class rc42dx
{
    public struct bxx
    {
        public byte[] xbyte;
    }
    public static bxx sfib(byte[] sr0)
    {
        bxx s;
        s.xbyte = new byte[256 * 256];
        s.xbyte[0] = sr0[0];
        s.xbyte[1] = sr0[1];
        s.xbyte[256] = sr0[2];
        s.xbyte[257] = sr0[3];
        for (byte n = 2; n < 255; n++)
        {
            s.xbyte[256 * (n) + 0] = (byte)((s.xbyte[256 * (n - 1) + 0] + s.xbyte[256 * (n - 2) + 0]) % 256);
            s.xbyte[256 * (0) + n] = (byte)((s.xbyte[256 * (0) + n - 1] + s.xbyte[256 * (0) + n - 2]) % 256);
            s.xbyte[256 * (n) + 1] = (byte)((s.xbyte[256 * (n - 1) + 0] + s.xbyte[256 * (n - 2) + 1]) % 256);
            s.xbyte[256 * (1) + n] = (byte)((s.xbyte[256 * (0) + n - 1] + s.xbyte[256 * (1) + n - 2]) % 256);
        }
        for (byte n = 2; n < 255; n++)
        {
            for (byte m = 2; m < 255; m++)
            {
                s.xbyte[256 * (n) + m] = (byte)((s.xbyte[256 * (n - 1) + m - 2] + s.xbyte[256 * (n - 2) + m - 1]) % 256);
            }
        }
        return s;
    }

    public static byte[] rc42d(byte[] xstr, byte[] xkey, byte[] ykey, bool sprime, byte[] sr0)
    {
        byte tjq;
        byte tiq;
        byte tjk;
        byte tik;
        int ex = 0;
        string sout = "";
        bxx s = sfib(sr0);
        Console.WriteLine("Box erstellt");
        byte j = 0;
        byte q = 0;
        for (byte z = 0; z < 255; z++)
        {
            for (byte y = 0; y < 255; y++)
            {
                j = (byte)((j + s.xbyte[256 * (z) + y] + (byte)(xkey[z % xkey.Length])) % 256);
                q = (byte)((q + s.xbyte[256 * (z) + y] + (byte)(ykey[y % ykey.Length])) % 256);
                tik = s.xbyte[256 * (z) + y];
                tjk = s.xbyte[256 * (j) + y];
                tiq = s.xbyte[256 * (z) + q];
                tjq = s.xbyte[256 * (j) + q];
                s.xbyte[256 * (z) + y] = tjk;
                s.xbyte[256 * (j) + y] = tiq;
                s.xbyte[256 * (j) + q] = tik;
                s.xbyte[256 * (z) + q] = tjq;
            }
        }
        byte[] mout = xstr;
        Console.WriteLine("Mixer fertig");
        j = 0;
        q = 0;
        byte i = 0;
        byte k = 0;
        for (int p = 0; p < xstr.Length; p++)
        {
            i = (byte)((i + 7) % 256);
            k = (byte)((k + 13) % 256);
            j = (byte)((j + s.xbyte[256 * (i) + k]) % 256);
            q = (byte)((q + s.xbyte[256 * (k) + i]) % 256);
            tik = s.xbyte[256 * (i) + k];
            tjk = s.xbyte[256 * (j) + k];
            tiq = s.xbyte[256 * (i) + q];
            tjq = s.xbyte[256 * (j) + q];
            s.xbyte[256 * (i) + k] = tiq;
            s.xbyte[256 * (j) + k] = tik;
            s.xbyte[256 * (j) + q] = tjq;
            s.xbyte[256 * (i) + q] = tjk;
            byte t1 = (byte)((s.xbyte[256 * (i) + k] + s.xbyte[256 * (j) + q]) % 256);
            byte t2 = (byte)((s.xbyte[256 * (i) + q] + s.xbyte[256 * (j) + k]) % 256);
            mout[p] = (byte)((byte)(xstr[p]) ^ s.xbyte[256 * (t1) + t2] ^ p % 256);
            if (sprime)
            {
                int xprime = 0;
                byte[] primes ={    2,     3,     5,     7,    11,    13,    17,    19,    23,    29,
                    31,    37,    41,    43, 47,    53,    59,    61,    67,    71,
                    73,    79,    83,    89,    97,   101,   103,   107, 109,   113,
                    127,   131,   137,   139,   149,   151,   157,   163,   167,   173,   179,   181};
                int w = p + s.xbyte[p % 65419];
                foreach (byte bprime in primes)
                {
                    xprime += (int)(w % bprime);
                }
                mout[p] = (byte)(mout[p] ^ ((byte)(xprime % 256)));
            }
            /*if (p%256==0){
              ex++;
              Console.WriteLine(p);
            }*/
        }
        return mout;
    }

    public static void encf(string file1, string file2, string xkey, string ykey, bool sprime)
    {
        byte[] encx = encb(File.ReadAllBytes(file1), xkey, ykey, sprime);
        File.WriteAllBytes(file2, encx);
    }
    public static byte[] encb(byte[] ori, string xkey, string ykey, bool sprime)
    {
        Random rand = new Random();
        Stopwatch timer = new Stopwatch();
        timer.Start();
        byte[] ran1x = new byte[16];
        rand.NextBytes(ran1x);
        byte[] ran2x = new byte[4];
        rand.NextBytes(ran2x);
        byte[] stan = { 0, 1, 2, 3 };
        byte[] ranx = (byte[])(ran1x.Concat(ran2x)).ToArray();
        byte[] ranxe = rc42d(ranx, Encoding.ASCII.GetBytes(xkey), Encoding.ASCII.GetBytes(ykey), sprime, stan);
        byte[] enco = rc42d(ori, Encoding.ASCII.GetBytes(xkey), ran1x, sprime, ran2x);

        timer.Stop();

        Console.WriteLine(timer.Elapsed.TotalMilliseconds.ToString("#,##0.00 'milliseconds'"));
        return (byte[])(ranxe.Concat(enco)).ToArray();
    }
    public static void deccf(string file1,string file2, string xkey, string ykey, bool sprime) {
        byte[] deccx = deccb(File.ReadAllBytes(file1), xkey, ykey, sprime);
        File.WriteAllBytes(file2, deccx);
    }
    public static byte[] deccb(byte[] ori, string xkey, string ykey, bool sprime)
    {
        Stopwatch timer = new Stopwatch();
        timer.Start();
        byte[] ranx = (byte[])(ori.Skip(0).Take(20)).ToArray();
        byte[] decx = (byte[])(ori.Skip(20)).ToArray();
        byte[] stan = { 0, 1, 2, 3 };
        byte[] ranxd = rc42d(ranx, Encoding.ASCII.GetBytes(xkey), Encoding.ASCII.GetBytes(ykey), sprime, stan);
        byte[] ran1x = (byte[])((ranxd.Skip(0).Take(16)).ToArray());
        byte[] ran2x = (byte[])(ranxd.Skip(16).Take(4)).ToArray();
        byte[] enco = rc42d(decx, Encoding.ASCII.GetBytes(xkey), ran1x, sprime, ran2x);

        timer.Stop();

        Console.WriteLine(timer.Elapsed.TotalMilliseconds.ToString("#,##0.00 'milliseconds'"));
        return enco;

    }
}
