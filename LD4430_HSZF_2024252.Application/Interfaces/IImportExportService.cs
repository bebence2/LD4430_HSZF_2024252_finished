using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LD4430_HSZF_2024252.Application
{
    public interface IImportExportService
    {
        void ImportJson(string filePath);
        void ExportJson(string filePath);
        void ExportTxt(string filePath);
    }
}
