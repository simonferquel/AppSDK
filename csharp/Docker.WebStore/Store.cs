using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Docker.WebStore
{
    public class Store
    {
        private readonly string _asmDir;
        private readonly string _tmpDir;
        private readonly Random _rand = new Random();

        private readonly SortedSet<Assembly> _assemblies = new SortedSet<Assembly>(Comparer<Assembly>.Create((lhs, rhs)=>string.Compare(lhs?.FullName, rhs?.FullName)));

        public Store(string asmDir, string tmpDir)
        {
            _asmDir = asmDir;
            _tmpDir = tmpDir;
            Initialize();
        }

        private void Initialize()
        {
            Directory.CreateDirectory(_asmDir);
            Directory.CreateDirectory(_tmpDir);

            foreach (var dll in Directory.EnumerateFiles(_asmDir, "*.dll")) {
                try {
                    var asm = Assembly.LoadFrom(dll);
                    _assemblies.Add(asm);
                } catch {
                    // not an assembly
                }
            }
        }

        private ( string path, Stream stream) CreateRandomDllFile(string dir)
        {
            for(; ;)
            {
                var path = Path.Combine(dir, _rand.Next(1000, 999999999).ToString() + ".dll");
                try {
                    var fs = File.Open(path, FileMode.CreateNew);
                    return (path, fs);
                } catch (IOException) {

                }
            }
        }

        public async Task LoadAsync(Stream s)
        {
            (var tmpPath, var tmpFs) = CreateRandomDllFile(_tmpDir);
            try {
                using (tmpFs) {
                    await s.CopyToAsync(tmpFs);
                    await tmpFs.FlushAsync();
                }

                var asm = Assembly.Load(File.ReadAllBytes(tmpPath));

                lock (_assemblies) {
                    if (!_assemblies.Add(asm)) {
                         //duplicate
                        return;
                    }
                }
                (var storePath, var storeFs) = CreateRandomDllFile(_asmDir);
                storeFs.Close();
                File.Copy(tmpPath, storePath, true);

            } finally {
                File.Delete(tmpPath);
            }

            

        }

        public IList<Assembly> List()
        {
            lock (_assemblies) {
                return _assemblies.OrderBy(asm => asm.FullName).ToList();
            }
        }
    }
}
