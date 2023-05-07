using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace ElpisOpcServer.SunPowerGen
{

    class PDFHeaderFooter : PdfPageEventHelper
    {
        internal dynamic testInformation { get; set; }
        //internal ReportGeneration reportGeneration { get; set; }
        public override void OnOpenDocument(PdfWriter writer, Document document) //OnStartPage
        {
            base.OnOpenDocument(writer, document);
            ReportGeneration reportgeneration = new ReportGeneration();
            reportgeneration.AddHeader(testInformation, document, writer);
            reportgeneration = null;
        }

        public override void OnEndPage(PdfWriter writer, Document document)
        {
            base.OnEndPage(writer, document);
            ReportGeneration reportgeneration = new ReportGeneration();
            reportgeneration.AddFooter(document, writer);
            reportgeneration = null;
        }
    }
}

