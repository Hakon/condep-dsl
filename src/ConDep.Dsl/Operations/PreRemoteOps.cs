﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using ConDep.Dsl.Config;
using ConDep.Dsl.Logging;
using ConDep.Dsl.PSScripts;
using ConDep.Dsl.Remote;
using ConDep.Dsl.Resources;
using ConDep.Dsl.SemanticModel;
using ConDep.Dsl.Validation;

namespace ConDep.Dsl.Operations
{
    internal class PreRemoteOps : IOperateRemote
    {
        const string TMP_FOLDER = @"{0}\temp\ConDep";
        const string NODE_LISTEN_URL = "http://{0}:80/ConDepNode/";

        public void Execute(ServerConfig server, IReportStatus status, ConDepSettings settings, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            Logger.WithLogSection("Pre-Operations", () =>
                {
                    server.GetServerInfo().TempFolderDos = string.Format(TMP_FOLDER, "%windir%");
                    Logger.Info(string.Format("Dos temp folder is {0}", server.GetServerInfo().TempFolderDos));

                    server.GetServerInfo().TempFolderPowerShell = string.Format(TMP_FOLDER, "$env:windir");
                    Logger.Info(string.Format("PowerShell temp folder is {0}", server.GetServerInfo().TempFolderPowerShell));

                    Logger.WithLogSection("Copying PowerShell Scripts", () => CopyResourceFiles(Assembly.GetExecutingAssembly(), PowerShellResources.PowerShellScriptResources, server, settings));

                    TempInstallConDepNode(server, settings);

                });

        }

        public bool IsValid(Notification notification)
        {
            return true;
        }

        private void CopyResourceFiles(Assembly assembly, IEnumerable<string> resources, ServerConfig server, ConDepSettings settings)
        {
            if (resources == null || assembly == null) return;

            var scriptPublisher = new PowerShellScriptPublisher(settings);
            scriptPublisher.PublishDslScripts(server);

            var src = Path.Combine(Path.GetDirectoryName(GetType().Assembly.Location), "ConDep.Dsl.Remote.Helpers.dll");
            var dst = string.Format(@"{0}\{1}", server.GetServerInfo().TempFolderPowerShell, "ConDep.Dsl.Remote.Helpers.dll");
            scriptPublisher.PublishFile(src, dst, server);
        }

        private void TempInstallConDepNode(ServerConfig server, ConDepSettings settings)
        {
            Logger.WithLogSection("Validating ConDepNode", () =>
                {
                    string path;

                    var executionPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "ConDepNode.exe");
                    if (!File.Exists(executionPath))
                    {
                        var currentPath = Path.Combine(Directory.GetCurrentDirectory(), "ConDepNode.exe");
                        if (!File.Exists(currentPath))
                        {
                            throw new FileNotFoundException("Could not find ConDepNode.exe. Paths tried: \n" +
                                                            executionPath + "\n" + currentPath);
                        }
                        path = currentPath;
                    }
                    else
                    {
                        path = executionPath;
                    }

                    var byteArray = File.ReadAllBytes(path);
                    var nodePublisher = new ConDepNodePublisher(byteArray, Path.Combine(server.GetServerInfo().OperatingSystem.ProgramFilesFolder, "ConDepNode", Path.GetFileName(path)), string.Format(NODE_LISTEN_URL, "localhost"), settings);
                    nodePublisher.Execute(server);
                    if (!nodePublisher.ValidateNode(string.Format(NODE_LISTEN_URL, server.Name), server.DeploymentUser.UserName, server.DeploymentUser.Password))
                    {
                        throw new ConDepNodeValidationException("Unable to make contact with ConDep Node or return content from API.");
                    }

                    Logger.Info(string.Format("ConDep Node successfully validated on {0}", server.Name));
                    Logger.Info(string.Format("Node listening on {0}", string.Format(NODE_LISTEN_URL, server.Name)));
                });
        }

        public string Name { get { return "Pre-Operation"; } }

        public void DryRun()
        {
            Logger.WithLogSection(Name, () => { });
        }
    }
}