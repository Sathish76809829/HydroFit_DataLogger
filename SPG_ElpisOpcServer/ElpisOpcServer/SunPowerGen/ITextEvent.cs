
using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace ElpisOpcServer.SunPowerGen
{
    public class ITextEvents : PdfPageEventHelper
    {
        internal dynamic testInformation { get; set; }
        // This is the contentbyte object of the writer
        PdfContentByte cb;

        // we will put the final number of pages in a template
        PdfTemplate headerTemplate, footerTemplate;

        // this is the BaseFont we are going to use for the header / footer
        BaseFont bf = null;

        // This keeps track of the creation time
        DateTime PrintTime = DateTime.Now;

        #region Fields
        private string _header;
        #endregion

        #region Properties
        public string Header
        {
            get { return _header; }
            set { _header = value; }
        }
        #endregion

        public override void OnOpenDocument(PdfWriter writer, Document document)
        {
            try
            {
                PrintTime = DateTime.Now;
                bf = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                cb = writer.DirectContent;
                headerTemplate = cb.CreateTemplate(100, 100);
                footerTemplate = cb.CreateTemplate(50, 50);
            }
            catch (DocumentException de)
            {
            }
            catch (System.IO.IOException ioe)
            {
            }
        }

        public override void OnEndPage(iTextSharp.text.pdf.PdfWriter writer, iTextSharp.text.Document document)
        {
            base.OnEndPage(writer, document);
            iTextSharp.text.Font baseFontNormal = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 12f, iTextSharp.text.Font.NORMAL, iTextSharp.text.BaseColor.BLACK);
            iTextSharp.text.Font baseFontBig = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 12f, iTextSharp.text.Font.BOLD, iTextSharp.text.BaseColor.BLACK);
           
            //var fontCollection = FontFactory.RegisteredFonts;
            Font arial_Heading = FontFactory.GetFont("Arial", 15,Font.BOLD, BaseColor.BLACK);
            Font arial_Content = FontFactory.GetFont("Arial", 11, BaseColor.BLACK);


            PdfPTable table = new PdfPTable(3);
            table.HorizontalAlignment = Element.ALIGN_CENTER;
            table.WidthPercentage = 98;
            table.SetWidths(new float[] { 1.14f, 1.4f, 2f });


            System.Windows.Controls.Image image = new System.Windows.Controls.Image();
            image = HomePage.image;
            image.Height = 110f;
            image.Width = 160;
            using (MemoryStream ms = new MemoryStream())
            {
                System.Windows.Media.Imaging.BmpBitmapEncoder bbe = new BmpBitmapEncoder();
                bbe.Frames.Add(BitmapFrame.Create(new Uri(image.Source.ToString(), UriKind.RelativeOrAbsolute)));

                bbe.Save(ms);
                System.Drawing.Image img2 = System.Drawing.Image.FromStream(ms);

                iTextSharp.text.Image logo = iTextSharp.text.Image.GetInstance(img2, BaseColor.BLACK);
                Font f = new Font(Font.FontFamily.COURIER, 15.0f, Font.BOLD, BaseColor.BLACK);
                Chunk c = new Chunk("SunPowerGen", f);
                logo.ScalePercent(50f);
                PdfPCell cell = new PdfPCell(logo);
                cell.HorizontalAlignment = PdfPCell.ALIGN_CENTER;
                cell.PaddingTop = 3;
                cell.PaddingBottom = 3;
                cell.VerticalAlignment = PdfPCell.ALIGN_CENTER;
                cell.Border = PdfPCell.BOX;
                cell.BorderWidth = 1.1f;
                cell.BorderColor = BaseColor.LIGHT_GRAY;
                table.AddCell(cell);


                Font f1 = new Font(Font.FontFamily.COURIER, 15.0f, Font.BOLD, BaseColor.BLACK);
                Chunk c1 = new Chunk("TEST REPORT", arial_Heading);

                cell = new PdfPCell(new Paragraph(c1)) { PaddingBottom = 5f, PaddingTop = 12f };
                cell.HorizontalAlignment = PdfPCell.ALIGN_CENTER;
                cell.VerticalAlignment = Element.ALIGN_CENTER;
                cell.Border = PdfPCell.BOX;
                cell.BorderWidth = 1.1f;
                cell.BorderColor = BaseColor.LIGHT_GRAY;
                table.AddCell(cell);

                PdfPTable ta1 = new PdfPTable(1);
                ta1.HorizontalAlignment = Element.ALIGN_CENTER;
                              
                PdfPTable table2 = new PdfPTable(2);
                table2.SetWidths(new float[] { 1, 1.6f });


                c = new Chunk("Test Date", arial_Content);// new Font(Font.FontFamily.TIMES_ROMAN, 11f, Font.BOLD, BaseColor.BLACK));
                cell = new PdfPCell(new Paragraph(c)) { PaddingTop = 2f,PaddingBottom=2f };// { Border = Rectangle.NO_BORDER, PaddingLeft = 15f };
                cell.BorderColor = BaseColor.LIGHT_GRAY;
                table2.AddCell(cell);

                cell = new PdfPCell(new Paragraph(DateTime.Today.ToShortDateString(),arial_Content )) { PaddingTop = 2f, PaddingBottom = 2f };// { Border = Rectangle.NO_BORDER };//new Font(Font.FontFamily.HELVETICA, 12f, Font.ITALIC, BaseColor.BLACK)
                cell.BorderColor = BaseColor.LIGHT_GRAY;
                table2.AddCell(cell);

                c = new Chunk("Customer Name", arial_Content);
                cell = new PdfPCell(new Paragraph(c)) { PaddingTop = 2f, PaddingBottom = 2f };// { Border = Rectangle.NO_BORDER, PaddingLeft = 15f };
                cell.BorderColor = BaseColor.LIGHT_GRAY;
                table2.AddCell(cell);

                cell = new PdfPCell(new Paragraph(testInformation.CustomerName.ToString(), arial_Content)) { PaddingTop = 2f, PaddingBottom = 2f };// { Border = Rectangle.NO_BORDER };               
                cell.BorderColor = BaseColor.LIGHT_GRAY;
                table2.AddCell(cell);

                c = new Chunk("Job Number", arial_Content) ;
                cell = new PdfPCell(new Paragraph(c)) { PaddingTop = 2f, PaddingBottom = 2f };// { Border = Rectangle.NO_BORDER, PaddingLeft = 15f };
                cell.BorderColor = BaseColor.LIGHT_GRAY;
                table2.AddCell(cell);

                cell = new PdfPCell(new Paragraph(testInformation.JobNumber.ToString(), arial_Content)) { PaddingTop = 2f, PaddingBottom = 2f };// { Border = Rectangle.NO_BORDER };                
                cell.BorderColor = BaseColor.LIGHT_GRAY;
                table2.AddCell(cell);

                table2.DefaultCell.Border = Rectangle.BOX;
                table2.DefaultCell.BorderColor = BaseColor.LIGHT_GRAY;
               
                cell = new PdfPCell(table2);
                cell.AddElement(table2);
                cell.Border = PdfPCell.BOX;
                cell.BorderWidth = 1.1f;
                cell.BorderColor = BaseColor.LIGHT_GRAY;
                table.AddCell(cell);
                table.TotalWidth = document.PageSize.Width - 60f;               
                table.WriteSelectedRows(0, -1, 30, document.PageSize.Height - 30, writer.DirectContent);
                //set pdfContent value

                ////Move the pointer and draw line to separate header section from rest of page
                //cb.MoveTo(40, document.PageSize.Height - 100);
                //cb.LineTo(document.PageSize.Width - 40, document.PageSize.Height - 100);
                //cb.Stroke();

                
                base.OnEndPage(writer, document);
                string website = string.Empty;
                string address = string.Empty;
                string refer = string.Empty;
                try
                {
                    website = string.IsNullOrEmpty(ConfigurationManager.AppSettings["WebSiteAddress"])?"": ConfigurationManager.AppSettings["WebSiteAddress"].ToString();
                    address = string.IsNullOrEmpty( ConfigurationManager.AppSettings["Address"])?"": ConfigurationManager.AppSettings["Address"].ToString();
                    refer = string.IsNullOrEmpty(ConfigurationManager.AppSettings["Ref"])?"": ConfigurationManager.AppSettings["Ref"].ToString();
                }
                catch (Exception) {

                }
                table = new PdfPTable(4);
                table.WidthPercentage = 90;
                table.TotalWidth = 300f;
                cell = new PdfPCell(new Paragraph(website, new Font(Font.FontFamily.COURIER, 14f, Font.BOLD, BaseColor.BLACK))) { Border = Rectangle.NO_BORDER, PaddingTop = 3, PaddingBottom = 3 };
                cell.Colspan = 4;
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                cell.VerticalAlignment = Element.ALIGN_CENTER;
                table.AddCell(cell);

                cell = new PdfPCell(new Paragraph(address, new Font(Font.FontFamily.TIMES_ROMAN, 10f, Font.NORMAL, BaseColor.BLACK))) { Border = Rectangle.NO_BORDER, PaddingTop = 1, PaddingBottom = 2 };
                cell.Colspan = 4;
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                cell.VerticalAlignment = Element.ALIGN_CENTER;
                table.AddCell(cell);

                cell = new PdfPCell(new Paragraph(refer, new Font(Font.FontFamily.TIMES_ROMAN, 8.5f, Font.NORMAL, BaseColor.DARK_GRAY))) { Border = Rectangle.NO_BORDER, PaddingTop = 2, PaddingBottom = 2 };
                cell.Colspan = 4;
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                cell.VerticalAlignment = Element.ALIGN_CENTER;
                table.AddCell(cell);
                table.DefaultCell.Border = Rectangle.TOP_BORDER;
                                

                table.WriteSelectedRows(0, -1, 150, document.Bottom+65, writer.DirectContent);

                //Move the pointer and draw line to separate footer section from rest of page
                cb.MoveTo(40, document.PageSize.GetBottom(75));
                cb.LineTo(document.PageSize.Width - 40, document.PageSize.GetBottom(75));
                cb.Stroke();
            }                    
        }

        public override void OnCloseDocument(PdfWriter writer, Document document)
        {
            base.OnCloseDocument(writer, document);

            headerTemplate.BeginText();
            headerTemplate.SetFontAndSize(bf, 12);
            headerTemplate.SetTextMatrix(0, 0);
            headerTemplate.ShowText((writer.PageNumber).ToString());
            headerTemplate.EndText();

            footerTemplate.BeginText();
            footerTemplate.SetFontAndSize(bf, 12);
            footerTemplate.SetTextMatrix(0, 0);
            footerTemplate.ShowText((writer.PageNumber).ToString());
            footerTemplate.EndText();
        }
    }
}