// Decompiled with JetBrains decompiler
// Type: Minecraft_Deobfuscator.Properties.Resources
// Assembly: Minecraft Deobfuscator, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: F4BAE0BF-CA2A-447E-86CC-7EDF2FB3D29B
// Assembly location: E:\business\me\projects\minecraft\Minecraft Deobfuscator\src-bin\Minecraft Deobfuscator.exe

using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace Minecraft_Deobfuscator.Properties
{
  [GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
  [DebuggerNonUserCode]
  [CompilerGenerated]
  internal class Resources
  {
    private static ResourceManager resourceMan;
    private static CultureInfo resourceCulture;

    internal Resources()
    {
    }

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    internal static ResourceManager ResourceManager
    {
      get
      {
        if (Minecraft_Deobfuscator.Properties.Resources.resourceMan == null)
          Minecraft_Deobfuscator.Properties.Resources.resourceMan = new ResourceManager("Minecraft_Deobfuscator.Properties.Resources", typeof (Minecraft_Deobfuscator.Properties.Resources).Assembly);
        return Minecraft_Deobfuscator.Properties.Resources.resourceMan;
      }
    }

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    internal static CultureInfo Culture
    {
      get
      {
        return Minecraft_Deobfuscator.Properties.Resources.resourceCulture;
      }
      set
      {
        Minecraft_Deobfuscator.Properties.Resources.resourceCulture = value;
      }
    }
  }
}
