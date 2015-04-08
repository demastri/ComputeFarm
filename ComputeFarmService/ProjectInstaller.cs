using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.Threading.Tasks;

namespace ComputeFarmService
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : System.Configuration.Install.Installer
    {
        public ProjectInstaller()
        {
            InitializeComponent();
            Console.WriteLine("-------------------------");
            foreach (Installer i in Installers)
                Console.WriteLine(i.ToString());
            Console.WriteLine("-------------------------");
        }
    }
}
