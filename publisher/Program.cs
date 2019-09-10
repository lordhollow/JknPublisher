using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace publisher
{
    static class Program
    {
        static Encoding encode = new UTF8Encoding(false);
        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static void Main()
        {
            var uriHere = new Uri(Application.StartupPath);
            var downer = new Uri(uriHere, @"..\..\..\..\");
            var basePath = downer.LocalPath;

            var templates = LoadTemplates(Path.Combine(basePath, @"_parts"));

            Resolve(basePath, templates);
        }
        /// <summary>
        /// テンプレートのロード
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        static private Dictionary<string, string> LoadTemplates(string p)
        {
            var ret = new Dictionary<string, string>();
            foreach(var file in Directory.GetFiles(p,"*.txt"))
            {
                var b = Path.GetFileNameWithoutExtension(file);

                ret[b] = File.ReadAllText(file);
            }
            return ret;
        }
        /// <summary>
        /// 置換
        /// テンプレートを @@{%...} の形で "*.t.html"ファイルに埋め込んでおくと、
        /// テンプレートを置換して"*.html"に出力する。
        /// 再帰的に処理を行うが、"_", "."で始まるフォルダ、およびresourceフォルダは対象外。
        /// </summary>
        /// <param name="p"></param>
        /// <param name="t"></param>
        static private void Resolve(string p, Dictionary<string, string> t)
        {
            Console.WriteLine("Resolving {0}...", p);
            var e = new Regex("@@{%([^}]+)}");
            foreach(var file in Directory.GetFiles(p, "*.t.html"))
            {
                var newFileName = file.Substring(0, file.Length - ".t.html".Length) + ".html";

                Console.WriteLine("{0} => {1}", file, newFileName);
                var b = File.ReadAllText(file);
                var tx = e.Replace(b, m =>
                 {
                     var key = m.Groups[1].Value;
                     if (t.ContainsKey(key))
                     {
                         return t[key];
                     }
                     else
                     {
                         return "";
                     }
                 });
                File.WriteAllText(newFileName, tx, encode);

                var date = File.GetLastWriteTime(file);
                File.SetLastWriteTime(newFileName, date);
            }
            //Recursive
            foreach(var dir in Directory.GetDirectories(p))
            {
                var name = Path.GetFileName(dir);
                if (name.StartsWith(".") || name.StartsWith("_") || name == "resource")
                {
                    Console.WriteLine("SKIP {0}", dir);
                }
                else
                {
                    Resolve(dir, t);
                }
            }

        }
    }
}
