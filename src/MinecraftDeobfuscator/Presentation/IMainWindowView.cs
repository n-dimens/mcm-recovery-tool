﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftModsDeobfuscator.Presentation {
    interface IMainWindowView {
        void UpdateMapping();

        void MappingFetched();

        void Deobfuscate_OnCompleted();
    }
}
