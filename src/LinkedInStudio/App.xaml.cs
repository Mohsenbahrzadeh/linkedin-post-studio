using System.Windows;
using LinkedInStudio.Services;

namespace LinkedInStudio;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        // Headless batch export (see HeadlessRunner for the supported options):
        //   LinkedInStudio.exe --render <output.png> [--in <project.lipost>]
        //   LinkedInStudio.exe --spec <files|folder> --out <dir> [--pdf <name.pdf>]
        if (HeadlessRunner.Handles(e.Args))
        {
            ShutdownMode = ShutdownMode.OnExplicitShutdown;
            // base.OnStartup is skipped on purpose: it would open MainWindow (StartupUri) in batch mode.
            try
            {
                HeadlessRunner.Run(e.Args);
            }
            finally
            {
                Shutdown();
            }
            return;
        }

        base.OnStartup(e);
    }
}
