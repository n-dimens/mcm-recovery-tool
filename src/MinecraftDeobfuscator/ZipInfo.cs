// Decompiled with JetBrains decompiler
// Type: Minecraft_Deobfuscator.ZipInfo
// Assembly: Minecraft Deobfuscator, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: F4BAE0BF-CA2A-447E-86CC-7EDF2FB3D29B
// Assembly location: E:\business\me\projects\minecraft\Minecraft Deobfuscator\src-bin\Minecraft Deobfuscator.exe

using System.IO;
using System.IO.Compression;

namespace Minecraft_Deobfuscator
{
  internal class ZipInfo
  {
    public string FullName { get; private set; }

    public byte[] EntryData { get; private set; }

    public ZipInfo(ZipArchiveEntry entry)
    {
      this.FullName = entry.FullName;
      this.EntryData = this.ReadEntry(entry.Open());
    }

    private byte[] ReadEntry(Stream input)
    {
      byte[] buffer = new byte[16384];
      using (MemoryStream memoryStream = new MemoryStream())
      {
        int count;
        while ((count = input.Read(buffer, 0, buffer.Length)) > 0)
          memoryStream.Write(buffer, 0, count);
        return memoryStream.ToArray();
      }
    }
  }
}
