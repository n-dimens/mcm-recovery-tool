using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftModsDeobfuscator.Presentation {
    class MainWindowPresenter {
        private readonly IMainWindowView view;

        public MainWindowPresenter(IMainWindowView view) {
            this.view = view;
        }
    }
}
