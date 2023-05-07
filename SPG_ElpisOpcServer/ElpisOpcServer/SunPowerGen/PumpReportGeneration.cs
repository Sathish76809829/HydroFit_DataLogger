using Elpis.Windows.OPC.Server;
using iTextSharp.text;
using iTextSharp.text.pdf;
using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ElpisOpcServer.SunPowerGen
{
    public class PumpReportGeneration
    {
        #region Properties
        internal static string FilePrefix = "PTR"; //ConfigurationSettings.AppSettings.Get["ReportPrefix"].ToString();
        private List<int> indexList = new List<int>();

        internal static string ReportLocation { get; set; }/// = string.IsNullOrEmpty(ConfigurationManager.AppSettings["ReportLocation"].ToString()) ? Directory.GetCurrentDirectory() : ConfigurationManager.AppSettings["ReportLocation"].ToString();
        internal static ObservableCollection<SeriesCollection> seriesCollection { get; set; }
        internal static ObservableCollection<List<string>> labelCollection { get; set; }
        #endregion

        public PumpReportGeneration()
        {
            if (string.IsNullOrEmpty(ReportLocation))
            {
                ReportLocation = ConfigurationManager.AppSettings["ReportLocation"].ToString();
                if (string.IsNullOrEmpty(ReportLocation) || !(Directory.Exists(ReportLocation)))
                {
                    ReportLocation = string.Format("{0}\\Reports_1", Directory.GetCurrentDirectory());
                }
            }

        }

        #region PDF File  related operation

        /// <summary>
        /// Adds the Report number, Date time to documnet.
        /// </summary>
        /// <param name="testInfo"></param>
        /// <param name="pdfDoc"></param>
        private PdfPTable AddEquipmentInformation(PumpTestInformation testInformation, Document pdfDoc)
        {
            try
            {
                Font arial_Heading = FontFactory.GetFont("Arial", 15, BaseColor.BLACK);
                Font arial_Content = FontFactory.GetFont("Arial", 11.5f, BaseColor.BLACK);

                Paragraph para2 = new Paragraph();
                PdfPTable table = new PdfPTable(2);
                PdfPCell cell = new PdfPCell(new Paragraph("EQUIPMENT UNDER TEST", new Font(Font.FontFamily.HELVETICA, 15f, Font.BOLD, BaseColor.BLACK))) { BackgroundColor = BaseColor.LIGHT_GRAY, PaddingTop = 5, PaddingBottom = 5, HorizontalAlignment = Element.ALIGN_CENTER };
                cell.Colspan = 2;
                table.AddCell(cell);

                Chunk c = new Chunk("Customer", new Font(Font.FontFamily.TIMES_ROMAN, 12f, Font.NORMAL, BaseColor.BLACK));
                cell = new PdfPCell(new Paragraph(c));// { Border = Rectangle.NO_BORDER };
                table.AddCell(cell);
                cell = new PdfPCell(new Paragraph(testInformation.EqipCustomerName.ToString(), new Font(Font.FontFamily.HELVETICA, 12f, Font.ITALIC, BaseColor.BLACK)));// { Border = Rectangle.NO_BORDER };
                table.AddCell(cell);

                c = new Chunk("Manufacturer", new Font(Font.FontFamily.TIMES_ROMAN, 12f, Font.NORMAL, BaseColor.BLACK));
                cell = new PdfPCell(new Paragraph(c));// { Border = Rectangle.NO_BORDER };
                table.AddCell(cell);
                cell = new PdfPCell(new Paragraph(testInformation.EquipManufacturer.ToString(), new Font(Font.FontFamily.HELVETICA, 12f, Font.ITALIC, BaseColor.BLACK)));// { Border = Rectangle.NO_BORDER };
                table.AddCell(cell);

                c = new Chunk("Type", new Font(Font.FontFamily.TIMES_ROMAN, 12f, Font.NORMAL, BaseColor.BLACK));
                cell = new PdfPCell(new Paragraph(c));// { Border = Rectangle.NO_BORDER };
                table.AddCell(cell);
                cell = new PdfPCell(new Paragraph(testInformation.EquipType.ToString(), new Font(Font.FontFamily.HELVETICA, 12f, Font.ITALIC, BaseColor.BLACK)));// { Border = Rectangle.NO_BORDER };
                table.AddCell(cell);



                c = new Chunk("Model No", new Font(Font.FontFamily.TIMES_ROMAN, 12f, Font.NORMAL, BaseColor.BLACK));
                cell = new PdfPCell(new Paragraph(c));// { Border = Rectangle.NO_BORDER };
                table.AddCell(cell);
                cell = new PdfPCell(new Paragraph(testInformation.EquipModelNo.ToString(), new Font(Font.FontFamily.HELVETICA, 12f, Font.ITALIC, BaseColor.BLACK)));// { Border = Rectangle.NO_BORDER };
                table.AddCell(cell);

                c = new Chunk("Serial No", new Font(Font.FontFamily.TIMES_ROMAN, 12f, Font.NORMAL, BaseColor.BLACK));
                cell = new PdfPCell(new Paragraph(c));// { Border = Rectangle.NO_BORDER };
                table.AddCell(cell);
                cell = new PdfPCell(new Paragraph(testInformation.EquipSerialNo.ToString(), new Font(Font.FontFamily.HELVETICA, 12f, Font.ITALIC, BaseColor.BLACK)));// { Border = Rectangle.NO_BORDER };
                table.AddCell(cell);

                c = new Chunk("Control Type", new Font(Font.FontFamily.TIMES_ROMAN, 12f, Font.NORMAL, BaseColor.BLACK));
                cell = new PdfPCell(new Paragraph(c));// { Border = Rectangle.NO_BORDER };
                table.AddCell(cell);
                cell = new PdfPCell(new Paragraph(testInformation.EquipControlType.ToString(), new Font(Font.FontFamily.HELVETICA, 12f, Font.ITALIC, BaseColor.BLACK)));// { Border = Rectangle.NO_BORDER };
                table.AddCell(cell);

                c = new Chunk("Pump Type", new Font(Font.FontFamily.TIMES_ROMAN, 12f, Font.NORMAL, BaseColor.BLACK));
                cell = new PdfPCell(new Paragraph(c));// { Border = Rectangle.NO_BORDER };
                table.AddCell(cell);
                cell = new PdfPCell(new Paragraph(testInformation.EquipPumpType.ToString(), new Font(Font.FontFamily.HELVETICA, 12f, Font.ITALIC, BaseColor.BLACK)));// { Border = Rectangle.NO_BORDER };
                table.AddCell(cell);

                PdfPCell cell1 = new PdfPCell(table);
                cell1.AddElement(table);
                PdfPTable table1 = new PdfPTable(1);
                table1.AddCell(cell1);
                return table1;
            }
            catch (Exception)
            {
                return null;
            }

        }

        public void GeneratePDFReport(string jobNumber, string reportNumber, bool createNew)
        {
            PumpTestInformation pumpTestData = HomePage.PumpTestInformation;
            string pdfFileName = null;
            Document pdfDoc = null;
            #region oldCode
            //old report code
            //try
            //{
            //    int fileCount = 0;
            //    pdfFileName = string.Format(@"{0}\{2}\{3}\{1}_{2}_{3}_PumpTest.pdf", ReportLocation, FilePrefix, jobNumber, reportNumber);
            //    Directory.CreateDirectory(string.Format(@"{0}\{1}\{2}", ReportLocation, jobNumber, reportNumber));
            //    pdfDoc = new Document(PageSize.A3, 9f, 9f, 10f, 10f);// 5, 5, 40, 35);// 10f, 10f, 140f, 10f);
            //    HomePage.PumpTestWindow.generateButtonClicked();
            //    if (File.Exists(pdfFileName))
            //    {

            //        MessageBoxResult messageOption = MessageBox.Show("Certificate with same name already exists.\nDo you want replace it?", "SPG Report Tool", MessageBoxButton.YesNo, MessageBoxImage.Question);

            //        if (messageOption == MessageBoxResult.Yes)
            //        {
            //            try
            //            {
            //                File.Delete(pdfFileName);
            //                using (PdfWriter pdfWriter = PdfWriter.GetInstance(pdfDoc, new FileStream(pdfFileName, System.IO.FileMode.Create)))
            //                {
            //                    Font arial = FontFactory.GetFont("Arial", 28, BaseColor.BLACK);



            //                    dynamic testInfo = null;
            //                    string[] file = Directory.GetFiles(string.Format("{0}\\{1}\\{2}", ReportLocation, jobNumber, reportNumber), "*.csv");
            //                    if (file.Length > 0)
            //                    {
            //                        testInfo = GetTestInformation(file[0], TestType.PumpTest) as PumpTestInformation;


            //                        pdfDoc.Open();
            //                        AddHeadertoCertificate(testInfo, pdfDoc);
            //                        Paragraph para = new Paragraph() { };
            //                        pdfDoc.Add(para);
            //                        PdfPTable table = new PdfPTable(1);
            //                        table.AddCell(new PdfPCell() { MinimumHeight = 10f, Border = Rectangle.NO_BORDER });

            //                        ObservableCollection<LineSeries> lineSeriesCollection = null;


            //                        string fileName = file[0];

            //                        if (!string.IsNullOrEmpty(fileName))
            //                        {

            //                            PumpTestInformation testInformation = GetTestInformation(fileName, TestType.PumpTest) as PumpTestInformation;
            //                            pdfDoc.Add(table);
            //                            PdfPTable t2 = new PdfPTable(2);
            //                            t2.WidthPercentage = 94;
            //                            PdfPTable EquipTable = AddEquipmentInformation(testInformation, pdfDoc);
            //                            t2.AddCell(EquipTable);
            //                            PdfPTable TestTable = AddTestBechInformation(testInformation, pdfDoc);
            //                            t2.AddCell(TestTable);
            //                            pdfDoc.Add(t2);
            //                            pdfDoc.Add(table);
            //                            lineSeriesCollection = GetSeriesCollection(fileName, testInformation.SeriesCounts, TestType.PumpTest);
            //                            seriesCollection = new ObservableCollection<SeriesCollection>();
            //                            SeriesCollection Series = new SeriesCollection();
            //                            foreach (var item in lineSeriesCollection)
            //                            {
            //                                Series = new SeriesCollection { item };
            //                                seriesCollection.Add(Series);
            //                            }


            //                            labelCollection = GetLabelCollection(fileName, testInformation.SeriesCounts);
            //                            PdfPTable tblDetail = new PdfPTable(2);
            //                            tblDetail.WidthPercentage = 94;
            //                            tblDetail.SetWidths(new float[] { 3, 1 });
            //                            PdfPTable graphTable = AddGraphstoCertificate(seriesCollection, labelCollection, pdfDoc, TestType.PumpTest, testInformation);
            //                            tblDetail.AddCell(graphTable);
            //                            PdfPTable detailtable = AddTestDeailsToCertificate(testInfo, pdfDoc, seriesCollection);
            //                            tblDetail.AddCell(detailtable);
            //                            pdfDoc.Add(tblDetail);
            //                            pdfDoc.Add(table);
            //                            AddVerificationDetails(testInfo, pdfDoc);
            //                            pdfDoc.Add(table);
            //                            AddFootertoCertificate(pdfDoc);

            //                            var content = pdfWriter.DirectContent;
            //                            var pageBorderRect = new Rectangle(pdfDoc.PageSize);

            //                            pageBorderRect.Left += pdfDoc.LeftMargin + 15;
            //                            pageBorderRect.Right -= pdfDoc.RightMargin + 15;
            //                            pageBorderRect.Top -= pdfDoc.TopMargin - 2;
            //                            pageBorderRect.Bottom += pdfDoc.BottomMargin - 2;

            //                            content.SetColorStroke(BaseColor.DARK_GRAY);
            //                            content.Rectangle(pageBorderRect.Left, pageBorderRect.Bottom, pageBorderRect.Width, pageBorderRect.Height);
            //                            content.Stroke();
            //                            HomePage.PumpTestInformation.ReportStatus = "Reports Generated Successfully.";



            //                        }

            //                        pdfDoc.Close();
            //                        ElpisOPCServerMainWindow.homePage.txtStatusMessage.Text = "Reports Generated Successfully. ";

            //                    }
            //                }

            //            }
            //            catch (Exception ex)
            //            {
            //                MessageBox.Show("File is Open");
            //                ElpisOPCServerMainWindow.homePage.txtStatusMessage.Text = "Pdf File is Open";
            //            }
            //        }
            //        else
            //        {
            //            ElpisOPCServerMainWindow.homePage.txtStatusMessage.Text = "";

            //        }
            //    }
            //    else
            //    {

            //        using (PdfWriter pdfWriter = PdfWriter.GetInstance(pdfDoc, new FileStream(pdfFileName, System.IO.FileMode.Create)))
            //        {
            //            Font arial = FontFactory.GetFont("Arial", 28, BaseColor.BLACK);



            //            dynamic testInfo = null;
            //            string[] file = Directory.GetFiles(string.Format("{0}\\{1}\\{2}", ReportLocation, jobNumber, reportNumber), "*.csv");
            //            if (file.Length > 0)
            //            {
            //                testInfo = GetTestInformation(file[0], TestType.PumpTest) as PumpTestInformation;


            //                pdfDoc.Open();
            //                AddHeadertoCertificate(testInfo, pdfDoc);
            //                Paragraph para = new Paragraph() { };
            //                pdfDoc.Add(para);
            //                PdfPTable table = new PdfPTable(1);
            //                table.AddCell(new PdfPCell() { MinimumHeight = 10f, Border = Rectangle.NO_BORDER });

            //                ObservableCollection<LineSeries> lineSeriesCollection = null;


            //                string fileName = file[0];

            //                if (!string.IsNullOrEmpty(fileName))
            //                {

            //                    PumpTestInformation testInformation = GetTestInformation(fileName, TestType.PumpTest) as PumpTestInformation;
            //                    pdfDoc.Add(table);
            //                    PdfPTable t2 = new PdfPTable(2);
            //                    t2.WidthPercentage = 94;
            //                    PdfPTable EquipTable = AddEquipmentInformation(testInformation, pdfDoc);
            //                    t2.AddCell(EquipTable);
            //                    PdfPTable TestTable = AddTestBechInformation(testInformation, pdfDoc);
            //                    t2.AddCell(TestTable);
            //                    pdfDoc.Add(t2);
            //                    pdfDoc.Add(table);
            //                    lineSeriesCollection = GetSeriesCollection(fileName, testInformation.SeriesCounts, TestType.PumpTest);
            //                    seriesCollection = new ObservableCollection<SeriesCollection>();
            //                    SeriesCollection Series = new SeriesCollection();
            //                    foreach (var item in lineSeriesCollection)
            //                    {
            //                        Series = new SeriesCollection { item };
            //                        seriesCollection.Add(Series);
            //                    }

            //                    labelCollection = GetLabelCollection(fileName, testInformation.SeriesCounts);
            //                    PdfPTable tblDetail = new PdfPTable(2);
            //                    tblDetail.WidthPercentage = 94;
            //                    tblDetail.SetWidths(new float[] { 3, 1 });
            //                    PdfPTable graphTable = AddGraphstoCertificate(seriesCollection, labelCollection, pdfDoc, TestType.PumpTest, testInformation);
            //                    tblDetail.AddCell(graphTable);
            //                    PdfPTable detailtable = AddTestDeailsToCertificate(testInfo, pdfDoc, seriesCollection);
            //                    tblDetail.AddCell(detailtable);
            //                    pdfDoc.Add(tblDetail);
            //                    pdfDoc.Add(table);
            //                    AddVerificationDetails(testInfo, pdfDoc);
            //                    pdfDoc.Add(table);
            //                    AddFootertoCertificate(pdfDoc);

            //                    var content = pdfWriter.DirectContent;
            //                    var pageBorderRect = new Rectangle(pdfDoc.PageSize);

            //                    pageBorderRect.Left += pdfDoc.LeftMargin + 15;
            //                    pageBorderRect.Right -= pdfDoc.RightMargin + 15;
            //                    pageBorderRect.Top -= pdfDoc.TopMargin - 2;
            //                    pageBorderRect.Bottom += pdfDoc.BottomMargin - 2;

            //                    content.SetColorStroke(BaseColor.DARK_GRAY);
            //                    content.Rectangle(pageBorderRect.Left, pageBorderRect.Bottom, pageBorderRect.Width, pageBorderRect.Height);
            //                    content.Stroke();
            //                    HomePage.PumpTestInformation.ReportStatus = "Reports Generated Successfully.";



            //                }

            //                pdfDoc.Close();
            //                ElpisOPCServerMainWindow.homePage.txtStatusMessage.Text = "Reports Generated Successfully. ";

            //            }
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{

            //}
            //finally
            //{
            //    if (pdfDoc != null && pdfDoc.IsOpen())
            //        pdfDoc.Close();
            //}
            #endregion
            
            //New report code
            try
            {
                //int fileCount = 0;
                pdfFileName = string.Format(@"{0}\{2}\{3}\{1}_{2}_{3}_PumpTest.pdf", ReportLocation, FilePrefix, jobNumber, reportNumber);
                Directory.CreateDirectory(string.Format(@"{0}\{1}\{2}", ReportLocation, jobNumber, reportNumber));
                pdfDoc = new Document(PageSize.A3, 9f, 9f, 10f, 10f); // 5, 5, 40, 35);// 10f, 10f, 140f, 10f);
                HomePage.PumpTestWindow.generateButtonClicked();
                if (File.Exists(pdfFileName))
                {

                    MessageBoxResult messageOption = MessageBox.Show("Certificate with same name already exists.\nDo you want replace it?", "SPG Report Tool", MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (messageOption == MessageBoxResult.Yes)
                    {
                        try
                        {
                            File.Delete(pdfFileName);
                            using (PdfWriter pdfWriter = PdfWriter.GetInstance(pdfDoc, new FileStream(pdfFileName, System.IO.FileMode.Create)))
                            {
                                Font arial = FontFactory.GetFont("Arial", 28, BaseColor.BLACK);



                                //dynamic testInfo = null;
                                //string[] file = Directory.GetFiles(string.Format("{0}\\{1}\\{2}", ReportLocation, jobNumber, reportNumber), "*.csv");
                                //if (file.Length > 0)
                                //{
                                // testInfo = GetTestInformation(file[0], TestType.PumpTest) as PumpTestInformation;


                                pdfDoc.Open();
                                AddHeadertoCertificate(pumpTestData, pdfDoc);
                                Paragraph para = new Paragraph() { };
                                pdfDoc.Add(para);
                                PdfPTable table = new PdfPTable(1);
                                table.AddCell(new PdfPCell() { MinimumHeight = 10f, Border = Rectangle.NO_BORDER });

                                ObservableCollection<LineSeries> lineSeriesCollection = null;


                                // string fileName = file[0];

                                //if (!string.IsNullOrEmpty(fileName))
                                //{
                                // PumpTestInformation testInformation = GetTestInformation(fileName, TestType.PumpTest) as PumpTestInformation;

                                pdfDoc.Add(table);
                                PdfPTable t2 = new PdfPTable(2);
                                t2.WidthPercentage = 94;
                                PdfPTable EquipTable = AddEquipmentInformation(pumpTestData, pdfDoc);
                                t2.AddCell(EquipTable);
                                PdfPTable TestTable = AddTestBechInformation(pumpTestData, pdfDoc);
                                t2.AddCell(TestTable);
                                pdfDoc.Add(t2);
                                pdfDoc.Add(table);
                                //lineSeriesCollection = GetSeriesCollection(fileName, testInformation.SeriesCounts, TestType.PumpTest);
                                seriesCollection = new ObservableCollection<SeriesCollection>();
                                SeriesCollection Series = new SeriesCollection();
                                foreach (var item in pumpTestData.LineSeriesList)
                                {
                                    Series = new SeriesCollection { item };
                                    seriesCollection.Add(Series);
                                }


                                // labelCollection = GetLabelCollection(fileName, testInformation.SeriesCounts);
                                PdfPTable tblGraph = new PdfPTable(1);
                                PdfPTable tblDetail = new PdfPTable(1);
                                tblGraph.WidthPercentage = 94;
                                tblDetail.WidthPercentage = 94;
                                //tblDetail.SetWidths(new float[] { 3, 1 });
                                PdfPTable graphTable = AddGraphstoCertificate(seriesCollection, pumpTestData.LabelCollection, pdfDoc, TestType.PumpTest, pumpTestData);
                                tblGraph.AddCell(graphTable);
                                PdfPTable detailtable = AddTestDeailsToCertificate(pumpTestData, pdfDoc);
                                tblDetail.AddCell(detailtable);
                                pdfDoc.Add(tblGraph);
                                pdfDoc.Add(tblDetail);
                                pdfDoc.Add(table);
                                AddVerificationDetails(pumpTestData, pdfDoc);
                                pdfDoc.Add(table);
                                AddFootertoCertificate(pdfDoc);

                                var content = pdfWriter.DirectContent;
                                var pageBorderRect = new Rectangle(pdfDoc.PageSize);

                                pageBorderRect.Left += pdfDoc.LeftMargin + 15;
                                pageBorderRect.Right -= pdfDoc.RightMargin + 15;
                                pageBorderRect.Top -= pdfDoc.TopMargin - 2;
                                pageBorderRect.Bottom += pdfDoc.BottomMargin - 2;

                                content.SetColorStroke(BaseColor.DARK_GRAY);
                                content.Rectangle(pageBorderRect.Left, pageBorderRect.Bottom, pageBorderRect.Width, pageBorderRect.Height);
                                content.Stroke();
                                HomePage.PumpTestInformation.ReportStatus = "Report Generated Successfully.";



                                //  }

                                pdfDoc.Close();
                                //ElpisOPCServerMainWindow.homePage.txtStatusMessage.Text = HomePage.PumpTestInformation.ReportStatus;
                                //ElpisOPCServerMainWindow.pump_Test.txtStatusMessage.Text = HomePage.PumpTestInformation.ReportStatus;
                                
                                //}
                            }

                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("File is Open");
                            //ElpisOPCServerMainWindow.homePage.txtStatusMessage.Text = "Pdf File is Open";
                            ElpisOPCServerMainWindow.pump_Test.txtStatusMessage.Text = "Pdf File is Open";
                        }
                    }
                    else
                    {
                        //ElpisOPCServerMainWindow.homePage.txtStatusMessage.Text = "";
                        ElpisOPCServerMainWindow.pump_Test.txtStatusMessage.Text = "";

                    }
                }
                else
                {

                    using (PdfWriter pdfWriter = PdfWriter.GetInstance(pdfDoc, new FileStream(pdfFileName, System.IO.FileMode.Create)))
                    {
                        Font arial = FontFactory.GetFont("Arial", 28, BaseColor.BLACK);



                        // dynamic testInfo = null;
                        // string[] file = Directory.GetFiles(string.Format("{0}\\{1}\\{2}", ReportLocation, jobNumber, reportNumber), "*.csv");
                        //if (file.Length > 0)
                        //{
                        //testInfo = GetTestInformation(file[0], TestType.PumpTest) as PumpTestInformation;


                        pdfDoc.Open();
                        AddHeadertoCertificate(pumpTestData, pdfDoc);
                        Paragraph para = new Paragraph() { };
                        pdfDoc.Add(para);
                        PdfPTable table = new PdfPTable(1);
                        table.AddCell(new PdfPCell() { MinimumHeight = 10f, Border = Rectangle.NO_BORDER });

                        //ObservableCollection<LineSeries> lineSeriesCollection = null;


                        //string fileName = file[0];

                        //if (!string.IsNullOrEmpty(fileName))
                        //{

                        // PumpTestInformation testInformation = GetTestInformation(fileName, TestType.PumpTest) as PumpTestInformation;
                        pdfDoc.Add(table);
                        PdfPTable t2 = new PdfPTable(2);
                        t2.WidthPercentage = 94;
                        PdfPTable EquipTable = AddEquipmentInformation(pumpTestData, pdfDoc);
                        t2.AddCell(EquipTable);
                        PdfPTable TestTable = AddTestBechInformation(pumpTestData, pdfDoc);
                        t2.AddCell(TestTable);
                        pdfDoc.Add(t2);
                        pdfDoc.Add(table);
                        // lineSeriesCollection = GetSeriesCollection(fileName, testInformation.SeriesCounts, TestType.PumpTest);
                        seriesCollection = new ObservableCollection<SeriesCollection>();
                        SeriesCollection Series = new SeriesCollection();
                        foreach (var item in pumpTestData.LineSeriesList)
                        {
                            Series = new SeriesCollection { item };
                            seriesCollection.Add(Series);
                        }

                        // labelCollection = GetLabelCollection(fileName, testInformation.SeriesCounts);
                        PdfPTable tblGraph = new PdfPTable(1);
                        PdfPTable tblDetail = new PdfPTable(1);
                        tblGraph.WidthPercentage = 94;
                        tblDetail.WidthPercentage = 94;
                        //tblDetail.SetWidths(new float[] { 3, 1 });
                        PdfPTable graphTable = AddGraphstoCertificate(seriesCollection, pumpTestData.LabelCollection, pdfDoc, TestType.PumpTest, pumpTestData);
                        tblGraph.AddCell(graphTable);
                        PdfPTable detailtable = AddTestDeailsToCertificate(pumpTestData, pdfDoc);
                        tblDetail.AddCell(detailtable);
                        pdfDoc.Add(tblGraph);
                        pdfDoc.Add(tblDetail);
                        pdfDoc.Add(table);
                        AddVerificationDetails(pumpTestData, pdfDoc);
                        pdfDoc.Add(table);
                        AddFootertoCertificate(pdfDoc);

                        var content = pdfWriter.DirectContent;
                        var pageBorderRect = new Rectangle(pdfDoc.PageSize);

                        pageBorderRect.Left += pdfDoc.LeftMargin + 15;
                        pageBorderRect.Right -= pdfDoc.RightMargin + 15;
                        pageBorderRect.Top -= pdfDoc.TopMargin - 2;
                        pageBorderRect.Bottom += pdfDoc.BottomMargin - 2;

                        content.SetColorStroke(BaseColor.DARK_GRAY);
                        content.Rectangle(pageBorderRect.Left, pageBorderRect.Bottom, pageBorderRect.Width, pageBorderRect.Height);
                        content.Stroke();
                        HomePage.PumpTestInformation.ReportStatus = "Report Generated Successfully.";



                        //  }

                        pdfDoc.Close();
                        //ElpisOPCServerMainWindow.homePage.txtStatusMessage.Text = HomePage.PumpTestInformation.ReportStatus;
                        //ElpisOPCServerMainWindow.pump_Test.txtStatusMessage.Text = HomePage.PumpTestInformation.ReportStatus;


                        //}
                    }
                }
            }
            catch (Exception ex)
            {

            }
            finally
            {
                if (pdfDoc != null && pdfDoc.IsOpen())
                    pdfDoc.Close();
            }

            ElpisOPCServerMainWindow.homePage.ReportTab.IsEnabled = true;
        }

        private void AddVerificationDetails(dynamic testInfo, Document pdfDoc)
        {

            Font arial_Heading = FontFactory.GetFont("Arial", 15, BaseColor.BLACK);
            Font arial_Content = FontFactory.GetFont("Arial", 11.5f, BaseColor.BLACK);
            Paragraph para2 = new Paragraph();
            PdfPTable table = new PdfPTable(3);
            table.WidthPercentage = 94;
            Chunk c = new Chunk("Tested By :"+testInfo.TestedBy, new Font(Font.FontFamily.TIMES_ROMAN, 11f, Font.BOLD, BaseColor.BLACK));
            PdfPCell cell = new PdfPCell(new Paragraph(c)) { Border = Rectangle.RECTANGLE, BorderWidth = 1f, BorderColor = BaseColor.BLACK, BorderColorTop = BaseColor.BLACK, BorderWidthTop = 1f };

            cell.FixedHeight = 25;
            table.AddCell(cell);
            c = new Chunk("Witnessed By :"+testInfo.WitnessedBy, new Font(Font.FontFamily.TIMES_ROMAN, 11f, Font.BOLD, BaseColor.BLACK));
            cell = new PdfPCell(new Paragraph(c)) { Border = Rectangle.RECTANGLE, BorderWidth = 1f, BorderColor = BaseColor.BLACK, BorderColorTop = BaseColor.BLACK, BorderWidthTop = 1f };

            cell.FixedHeight = 25;
            table.AddCell(cell);
            c = new Chunk("Approved By :"+testInfo.ApprovedBy, new Font(Font.FontFamily.TIMES_ROMAN, 11f, Font.BOLD, BaseColor.BLACK));
            cell = new PdfPCell(new Paragraph(c)) { Border = Rectangle.RECTANGLE, BorderWidth = 1f, BorderColor = BaseColor.BLACK, BorderColorTop = BaseColor.BLACK, BorderWidthTop = 1f };

            cell.FixedHeight = 25;
            table.AddCell(cell);
            cell = new PdfPCell() { Border = Rectangle.RECTANGLE, BorderWidth = 1f, BorderColor = BaseColor.BLACK };

            cell.FixedHeight = 30;
            table.AddCell(cell);
            cell = new PdfPCell() { Border = Rectangle.RECTANGLE, BorderWidth = 1f, BorderColor = BaseColor.BLACK };

            cell.FixedHeight = 30;
            table.AddCell(cell);
            cell = new PdfPCell() { Border = Rectangle.RECTANGLE, BorderWidth = 1f, BorderColor = BaseColor.BLACK };

            cell.FixedHeight = 30;
            table.AddCell(cell);
            pdfDoc.Add(table);
        }

        private PdfPTable AddTestDeailsToCertificate(PumpTestInformation testInfo, Document pdfDoc)
        {
            try
            {
                Font arial_Heading = FontFactory.GetFont("Arial", 15, BaseColor.BLACK);
                Font arial_Content = FontFactory.GetFont("Arial", 11.5f, BaseColor.BLACK);

                Paragraph para2 = new Paragraph();
                PdfPTable table = new PdfPTable(1);
                PdfPCell cell = new PdfPCell(new Paragraph("Test Details", new Font(Font.FontFamily.HELVETICA, 15f, Font.BOLD, BaseColor.BLACK))) { BackgroundColor = BaseColor.LIGHT_GRAY, PaddingTop = 5, PaddingBottom = 5, HorizontalAlignment = Element.ALIGN_CENTER };
                table.AddCell(cell);
                PdfPTable tbldetails = new PdfPTable(3);
                tbldetails.SetWidths(new float[] { 1.5f, 3, 1.5f });// 1,1,1
                tbldetails.AddCell(AddTestDetailsBox
                    (pdfDoc, HomePage.PumpTestInformation.TestDeatilsInfo));

                #region table old code
                //if (testInfo.TableParameterList.Count > 0 && testInfo.TableData.Keys != null)
                //{
                //    PdfPTable seriesTable = new PdfPTable(testInfo.TableParameterList.Count);
                //    Dictionary<string, List<double>> data = new Dictionary<string, List<double>>();
                //    foreach (var item in seriesCollection)
                //    {
                //        if (testInfo.TableParameterList.Contains(item[0].Title))
                //        {
                //            data.Add(item[0].Title, new List<double>());
                //            seriesTable.AddCell(item[0].Title);
                //        }
                //    }


                //    //foreach (var item in seriesCollection)
                //    //{
                //    //    for (int i = 0; i < item[0].Values.Count; i++)
                //    //    {
                //    //        seriesTable.AddCell(item[0].Values[i].ToString());
                //    //    }
                //    //}
                //    var SeriesValueCount = seriesCollection.First()[0].Values.Count;
                //    int cunt = 0;
                //    indexList.Clear();

                //    foreach (var item in seriesCollection)
                //    {
                //        if (testInfo.TableParameterList.Contains(item[0].Title))
                //        {
                //            var index = seriesCollection.IndexOf(item);
                //            if (index == 0)
                //            {
                //                foreach (var value in item[0].Values)
                //                {
                //                    var list = data.FirstOrDefault(e => e.Key == item[0].Title);
                //                    if (list.Value.Count == 0)
                //                    {

                //                        indexList.Add(item[0].Values.IndexOf(value));
                //                        list.Value.Add(Convert.ToDouble(value));

                //                    }
                //                    else
                //                    {

                //                        var lastData = list.Value.Last();
                //                        if ((Convert.ToDouble(value) - lastData) > 25 && Convert.ToDouble(value) > lastData)
                //                        {
                //                            indexList.Add(item[0].Values.IndexOf(value));
                //                            list.Value.Add(Convert.ToDouble(value));

                //                        }
                //                        else if (Convert.ToDouble(value) < lastData && (lastData - Convert.ToDouble(value)) > 25)
                //                        {
                //                            indexList.Add(item[0].Values.IndexOf(value));
                //                            list.Value.Add(Convert.ToDouble(value));

                //                        }
                //                    }
                //                }
                //            }
                //            else
                //            {

                //                foreach (var itemIndex in indexList)
                //                {
                //                    var list = data.FirstOrDefault(e => e.Key == item[0].Title);
                //                    list.Value.Add(Convert.ToDouble(item[0].Values[itemIndex]));
                //                }

                //            }

                //        }
                //    }

                //    while (cunt < data.First().Value.Count)
                //    {
                //        foreach (var item in data)
                //        {
                //            if (testInfo.TableParameterList.Contains(item.Key))
                //            {
                //                seriesTable.AddCell(item.Value[cunt].ToString());

                //            }
                //        }
                //        cunt++;
                //    }


                //    table.AddCell(seriesTable);
                //    table.AddCell(new PdfPCell() { MinimumHeight = 10f, Border = Rectangle.NO_BORDER });

                //}
                #endregion

                #region table new code
                if (testInfo.TableParameterList.Count > 0 && testInfo.TableData.Keys != null)
                {
                    PdfPTable seriesTable = new PdfPTable(testInfo.TableParameterList.Count);
                    Dictionary<string, List<string>> data = new Dictionary<string, List<string>>();
                    foreach (var item in testInfo.TableData.Keys)
                    {
                        
                        data.Add(item, new List<string>());
                        seriesTable.AddCell(item);

                    }



                    int cunt = 0;

                    foreach (var item in testInfo.TableData)
                    {
                        if (testInfo.TableParameterList.Contains(item.Key))
                        {
                            var index = testInfo.TableData.ToList().IndexOf(item);
                            //if (index == 0)
                            //{
                                var values = item.Value;
                                foreach (var subKey in values.Keys)
                                {
                                    var list = data.FirstOrDefault(e => e.Key == item.Key);
                                   // indexList.Add(i);
                                    list.Value.Add(values[subKey]);
                                    //if (list.Value.Count == 0)
                                    //{

                                    //    indexList.Add(item.Value.IndexOf(value));
                                    //    list.Value.Add(Convert.ToDouble(value));

                                    //}
                                    //else
                                    //{

                                    //    var lastData = list.Value.Last();
                                    //    if ((Convert.ToDouble(value) - lastData) > 25 && Convert.ToDouble(value) > lastData)
                                    //    {
                                    //        indexList.Add(item.Value.IndexOf(value));
                                    //        list.Value.Add(Convert.ToDouble(value));

                                    //    }
                                    //    else if (Convert.ToDouble(value) < lastData && (lastData - Convert.ToDouble(value)) > 25)
                                    //    {
                                    //        indexList.Add(item.Value.IndexOf(value));
                                    //        list.Value.Add(Convert.ToDouble(value));

                                    //    }
                                    //}
                                }
                            //}
                            //else
                            //{

                            //    foreach (var itemIndex in indexList)
                            //    {
                            //        var list = data.FirstOrDefault(e => e.Key == item.Key);
                            //        list.Value.Add(Convert.ToDouble(item.Value[item.Key])));
                            //    }

                            //}

                        }
                    }
                    
                    while (cunt < data.First().Value.Count)
                    {
                        foreach (var item in data)
                        {
                            if (testInfo.TableParameterList.Contains(item.Key))
                            {
                                seriesTable.AddCell(item.Value[cunt].ToString());

                            }
                        }
                        cunt++;
                    }
                   


                    tbldetails.AddCell(seriesTable);

                }
                #endregion


                //if (HomePage.PumpTestInformation.Comment != "")
                //{
                tbldetails.AddCell(AddCommnetBox(pdfDoc, HomePage.PumpTestInformation.Comment));
                //}
                table.AddCell(tbldetails);
                return table;

            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message);
                return null;
            }
        }

        private ObservableCollection<List<string>> GetLabelCollection(string fileName, int numberofLabels)
        {
            ObservableCollection<List<string>> labelCollection = new ObservableCollection<List<string>>();
            bool isDataLineFound = false;
            if (File.Exists(fileName))
            {
                using (FileStream stream = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        while (!reader.EndOfStream)
                        {
                            string line = reader.ReadLine();
                            if (line.Split(',')[0] == "" && (!isDataLineFound))
                                isDataLineFound = true;
                            if (line.Split(',').Length == (numberofLabels * 2) && isDataLineFound)
                            {
                                if (labelCollection.Count == 0)
                                    line = reader.ReadLine();
                                string[] data = line.Split(',');
                                bool isValid = false;
                                int totalColums = numberofLabels * 2;
                                for (int i = totalColums - 1; i >= 1; i = i - 2)
                                {
                                    if (data[i] != "")
                                    {
                                        isValid = true;
                                    }
                                    else
                                    {
                                        isValid = false;
                                    }
                                }



                                //if (numberofLabels == 4)
                                //{
                                //    isValid = data[1] != "" && data[3] != "" && data[5] != "" && data[7] != "";
                                //}
                                //if (numberofLabels == 3)
                                //{
                                //    isValid = data[1] != "" && data[3] != "" && data[5] != "";
                                //}
                                //else if (numberofLabels == 2)
                                //{
                                //    isValid = data[1] != "" && data[3] != "";
                                //}
                                //else if (numberofLabels == 1)
                                //{
                                //    isValid = data[1] != "";
                                //}
                                if (isValid)
                                {
                                    for (int i = 0; i < data.Length; i = i + 2)
                                    {
                                        //int count = Regex.Matches(data[i], @"[a-zA-Z]").Count;
                                        //if (count == 0)
                                        //{
                                        if (labelCollection.Count >= numberofLabels)
                                        {
                                            labelCollection[((i) / 2)].Add(data[i]);
                                        }
                                        else
                                        {
                                            List<string> labels = new List<string>();
                                            labels.Add(data[i]);
                                            labelCollection.Add(labels);
                                        }
                                        //}
                                    }
                                }
                            }
                        }
                    }
                    return labelCollection;
                }
            }
            return null;
        }

        private ObservableCollection<LineSeries> GetSeriesCollection(string fileName, int numberofSeries, TestType pumpTest)
        {
            ObservableCollection<LineSeries> seriesCollection = new ObservableCollection<LineSeries>();
            Brush[] graphStrokes = new Brush[] { Brushes.DarkOrange, Brushes.DarkGreen, Brushes.DarkBlue, Brushes.Brown, Brushes.Red, Brushes.DarkCyan, Brushes.DarkMagenta, Brushes.DarkOliveGreen, Brushes.DarkOrange, Brushes.DarkSalmon };

            List<string> seriesNames = new List<string>();
            int nameIndex = 0;
            bool isDataLineFound = false;

            if (File.Exists(fileName))
            {
                using (FileStream stream = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {

                        while (!reader.EndOfStream)
                        {
                            string line = reader.ReadLine();
                            string[] data = line.Split(',');

                            if (data[0] == "" && (!isDataLineFound))
                                isDataLineFound = true;
                            if (data.Length == (numberofSeries * 2) && isDataLineFound)
                            {
                                bool isValid = false;
                                int totalColums = numberofSeries * 2;
                                for (int i = 1; i < totalColums; i = i + 2)
                                {
                                    if (data[i - 1] == "" && seriesNames.Count <= numberofSeries)
                                    {
                                        if (data[i] != null)
                                        {
                                            seriesNames.Add(data[i]);
                                        }
                                    }

                                }

                                for (int i = totalColums - 2; i >= 0; i = i - 2)
                                {

                                    if (data[i] != "")
                                    {
                                        isValid = true;
                                    }
                                    else
                                    {
                                        isValid = false;
                                    }
                                }


                                if (isValid)
                                {
                                    for (int i = 1; i < data.Length; i = i + 2)
                                    {
                                        if (seriesCollection.Count >= numberofSeries)
                                        {
                                            seriesCollection[((i - 1) / 2)].Values.Add(double.Parse(data[i]));
                                        }
                                        else
                                        {
                                            LineSeries series = new LineSeries() { Values = new ChartValues<double>(), Stroke = graphStrokes[nameIndex], Title = seriesNames[nameIndex], PointGeometrySize = 5, StrokeThickness =1};
                                            series.Values.Add(double.Parse(data[i]));
                                            seriesCollection.Add(series);
                                            nameIndex++;
                                        }
                                    }
                                }

                            }
                        }
                        return seriesCollection;
                    }

                }
            }
            return null;
        }

        private PumpTestInformation GetTestInformation(string fileName, TestType pumpTest)
        {
            //dynamic testInormation = TestTypeFactory.CreateTestInformationObject(pumpTest);

            if (File.Exists(fileName))
            {
                using (FileStream stream = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        PumpTestInformation PumpTestInfo = new PumpTestInformation();
                        while (!reader.EndOfStream)
                        {
                            string line = reader.ReadLine();
                            if (line != "")
                            {
                                if (line.Split(',')[0].ToLower() == "customername")
                                    PumpTestInfo.EqipCustomerName = (line.Split(',')[1]);
                                if (line.Split(',')[0].ToLower() == "jobnumber")
                                    PumpTestInfo.PumpJobNumber = (line.Split(',')[1]);
                                else if (line.Split(',')[0].ToLower() == "reportnumber")
                                    PumpTestInfo.PumpReportNumber = (line.Split(',')[1]);
                                else if (line.Split(',')[0].ToLower() == "spgserialno")
                                    PumpTestInfo.PumpSPGSerialNo = (line.Split(',')[1]);
                                else if (line.Split(',')[0].ToLower() == "equip type")
                                    PumpTestInfo.EquipType = line.Split(',')[1];
                                else if (line.Split(',')[0].ToLower() == "equip manufacturer")
                                    PumpTestInfo.EquipManufacturer = line.Split(',')[1];
                                else if (line.Split(',')[0].ToLower() == "equip modelno")
                                    PumpTestInfo.EquipModelNo = line.Split(',')[1];
                                else if (line.Split(',')[0].ToLower() == "equip serialno")
                                    PumpTestInfo.EquipSerialNo = line.Split(',')[1];
                                else if (line.Split(',')[0].ToLower() == "equip controltype")
                                    PumpTestInfo.EquipControlType = line.Split(',')[1];
                                else if (line.Split(',')[0].ToLower() == "equip pumptype")
                                    PumpTestInfo.EquipPumpType = line.Split(',')[1];
                                else if (line.Split(',')[0].ToLower() == "test manufacture")
                                    PumpTestInfo.TestManufacture = line.Split(',')[1];
                                else if (line.Split(',')[0].ToLower() == "test type")
                                    PumpTestInfo.TestType = line.Split(',')[1];
                                else if (line.Split(',')[0].ToLower() == "test serialno")
                                    PumpTestInfo.TestSerialNo = line.Split(',')[1];
                                else if (line.Split(',')[0].ToLower() == "test range")
                                    PumpTestInfo.TestRange = line.Split(',')[1];
                                else if (line.Split(',')[0].ToLower() == "testedby")
                                    PumpTestInfo.TestedBy = line.Split(',')[1];
                                else if (line.Split(',')[0].ToLower() == "witnessedby")
                                    PumpTestInfo.WitnessedBy = line.Split(',')[1];
                                else if (line.Split(',')[0].ToLower() == "opprovedby")
                                    PumpTestInfo.ApprovedBy = line.Split(',')[1];
                                else if (line.Split(',')[0].ToLower() == "series count")
                                    PumpTestInfo.SeriesCounts = Convert.ToInt16(line.Split(',')[1]);
                                else if (line.Split(',')[0].ToLower() == "selected xaxis")
                                    PumpTestInfo.SelectedXaxis = line.Split(',')[1];
                                else if (line.Split(',')[0].ToLower() == "table para")
                                {
                                    foreach (var item in line.Split(',')[1].Split(':'))
                                    {
                                        PumpTestInfo.TableParameterList.Add(item);
                                    }
                                }

                                else if (line.Split(',')[0].ToLower() == "test date")
                                {
                                    PumpTestInfo.TestDateTime = line.Split(',')[1];
                                }
                            }
                        }
                        return PumpTestInfo;
                    }
                }


            }
            return null;
        }

        private PdfPTable AddTestBechInformation(PumpTestInformation testInformation, Document pdfDoc)
        {
            try
            {
                Font arial_Heading = FontFactory.GetFont("Arial", 15, BaseColor.BLACK);
                Font arial_Content = FontFactory.GetFont("Arial", 11.5f, BaseColor.BLACK);

                Paragraph para2 = new Paragraph();
                PdfPTable table = new PdfPTable(2);
                PdfPCell cell = new PdfPCell(new Paragraph("TEST BENCH DETAILS", new Font(Font.FontFamily.HELVETICA, 15f, Font.BOLD, BaseColor.BLACK))) { BackgroundColor = BaseColor.LIGHT_GRAY, PaddingTop = 5, PaddingBottom = 5, HorizontalAlignment = Element.ALIGN_CENTER };
                cell.Colspan = 2;
                table.AddCell(cell);

                Chunk c = new Chunk("Manufacturer", new Font(Font.FontFamily.TIMES_ROMAN, 12f, Font.NORMAL, BaseColor.BLACK));
                cell = new PdfPCell(new Paragraph(c));// { Border = Rectangle.NO_BORDER };
                table.AddCell(cell);
                cell = new PdfPCell(new Paragraph(testInformation.TestManufacture.ToString(), new Font(Font.FontFamily.HELVETICA, 12f, Font.ITALIC, BaseColor.BLACK)));// { Border = Rectangle.NO_BORDER };
                table.AddCell(cell);

                c = new Chunk("Type", new Font(Font.FontFamily.TIMES_ROMAN, 12f, Font.NORMAL, BaseColor.BLACK));
                cell = new PdfPCell(new Paragraph(c));// { Border = Rectangle.NO_BORDER };
                table.AddCell(cell);
                cell = new PdfPCell(new Paragraph(testInformation.TestType.ToString(), new Font(Font.FontFamily.HELVETICA, 12f, Font.ITALIC, BaseColor.BLACK)));// { Border = Rectangle.NO_BORDER };
                table.AddCell(cell);

                c = new Chunk("Serial No", new Font(Font.FontFamily.TIMES_ROMAN, 12f, Font.NORMAL, BaseColor.BLACK));
                cell = new PdfPCell(new Paragraph(c));// { Border = Rectangle.NO_BORDER };
                table.AddCell(cell);
                cell = new PdfPCell(new Paragraph(testInformation.TestSerialNo.ToString(), new Font(Font.FontFamily.HELVETICA, 12f, Font.ITALIC, BaseColor.BLACK)));// { Border = Rectangle.NO_BORDER };
                table.AddCell(cell);



                c = new Chunk("Range", new Font(Font.FontFamily.TIMES_ROMAN, 12f, Font.NORMAL, BaseColor.BLACK));
                cell = new PdfPCell(new Paragraph(c));// { Border = Rectangle.NO_BORDER };
                table.AddCell(cell);
                cell = new PdfPCell(new Paragraph(testInformation.TestRange.ToString(), new Font(Font.FontFamily.HELVETICA, 12f, Font.ITALIC, BaseColor.BLACK)));// { Border = Rectangle.NO_BORDER };
                table.AddCell(cell);


               
                PdfPCell cell1 = new PdfPCell(table);
                cell1.AddElement(table);
                PdfPTable table1 = new PdfPTable(1);
                table1.AddCell(cell1);
                return table1;
            }
            catch (Exception)
            {
                return null;
            }

        }

        private PdfPCell AddTestDetailsBox(Document pdfDoc, string comment)
        {
            try
            {

                Font arial_Heading = FontFactory.GetFont("Arial", 10f, Font.NORMAL, BaseColor.BLACK);
                Paragraph para = new Paragraph();
                string note = "";
                if (comment != null)
                    note = "\u2022" + comment.Replace("\r\n", "\r\n\u2022");
                PdfPCell cell = new PdfPCell(new Paragraph("Details :\r\n" + note)) { Border = Rectangle.NO_BORDER, VerticalAlignment = Element.ALIGN_TOP };
                return cell;


            }
            catch (Exception)
            {
                return null;
            }

        }



        private PdfPCell AddCommnetBox(Document pdfDoc, string comment)
        {
            try
            {

                Font arial_Heading = FontFactory.GetFont("Arial", 10f, Font.NORMAL, BaseColor.BLACK);
                string note = "";
                if (comment != null && comment != "")
                    note = "\u2022" + comment.Replace("\r\n", "\r\n\u2022");
                PdfPCell cell = new PdfPCell(new Paragraph("NOTE :\r\n" + note)) { Border = Rectangle.NO_BORDER, VerticalAlignment = Element.ALIGN_TOP};
                return cell;

            }
            catch (Exception)
            {
                return null;
            }

        }


        /// <summary>
        /// This Method Adds Graphs to the PDF File.
        /// </summary>
        /// <param name="seriesCollection"></param>
        /// <param name="labelCollection"></param>
        /// <param name="pdfDoc"></param>
        private PdfPTable AddGraphstoCertificate(ObservableCollection<SeriesCollection> seriesCollection, ObservableCollection<List<string>> labelCollection, Document pdfDoc, TestType testType, PumpTestInformation pumpTestInformation)
        {
            try
            {
                Font arial_Heading = FontFactory.GetFont("Arial", 18, BaseColor.BLACK);
                Font arial_Content = FontFactory.GetFont("Arial", 15, BaseColor.BLACK);


                if (seriesCollection != null && labelCollection != null)
                {
                    PdfPTable table = new PdfPTable(1);
                    PdfPCell cell = new PdfPCell(new Paragraph("Graph Information", arial_Heading)) { BackgroundColor = BaseColor.LIGHT_GRAY, PaddingTop = 5, PaddingBottom = 5, VerticalAlignment = Element.ALIGN_CENTER, HorizontalAlignment = Element.ALIGN_CENTER };//, PaddingLeft = -10, PaddingRight = -10 };
                    cell.Colspan = seriesCollection.Count;
                    table.AddCell(cell);
                    cell = new PdfPCell() { MinimumHeight = 10, Border = Rectangle.NO_BORDER };
                    //cell.Colspan = seriesCollection.Count;
                    table.AddCell(cell);
                    // table = new PdfPTable(seriesCollection.Count);
                    table.WidthPercentage = 80;

                    //for (int i = 0; i < seriesCollection.Count; i++)
                    //{
                    string imageFile = string.Format("{0}\\PumpChart.png", Directory.GetCurrentDirectory());


                    //bool isCreated = GenerateImageFromGraph(seriesCollection, labelCollection, imageFile, testType, seriesCollection.Count, pumpTestInformation);
                    if (/*isCreated*/imageFile != null)
                    {
                        iTextSharp.text.Image graph = iTextSharp.text.Image.GetInstance(imageFile);
                        graph.ScaleAbsolute(760, 450);
                        //graph.ScalePercent(150f);
                        cell = new PdfPCell(graph);
                        table.AddCell(cell);
                        File.Delete(imageFile);
                    }

                    //}
                    return table;
                    // MessageBox.Show("Graphs Added");
                }
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }


        /// <summary>
        /// This Method Add Header(Logo,Test DataTime) to the PDF File.
        /// </summary>
        /// <param name="testInfo"></param>
        /// <param name="pdfDoc"></param>
        private void AddHeadertoCertificate(dynamic testInfo, Document pdfDoc)
        {
            // Paragraph para1 = new Paragraph() { };
            // LogoImages(pdfDoc);
            PdfPTable table = new PdfPTable(3);
            table.HorizontalAlignment = Element.ALIGN_CENTER;
            table.WidthPercentage = 94;
            table.SetWidths(new float[] { 1.12f, 1.5f, 2f });


            System.Windows.Controls.Image image = new System.Windows.Controls.Image();
            image = HomePage.image;
            image.Height = 110f;
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


                Font f1 = new Font(Font.FontFamily.COURIER, 24.0f, Font.BOLD, BaseColor.BLACK);
                Chunk c1 = new Chunk("PUMP TEST REPORT", f1);

                cell = new PdfPCell(new Paragraph(c1)) { PaddingBottom = 5f, PaddingTop = 5f };
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


                c = new Chunk("Report Date", new Font(Font.FontFamily.TIMES_ROMAN, 11f, Font.BOLD, BaseColor.BLACK));
                cell = new PdfPCell(new Paragraph(c));// { Border = Rectangle.NO_BORDER, PaddingLeft = 15f };
                cell.BorderColor = BaseColor.LIGHT_GRAY;
                table2.AddCell(cell);

                cell = new PdfPCell(new Paragraph(DateTime.Today.ToShortDateString(), new Font(Font.FontFamily.HELVETICA, 12f, Font.ITALIC, BaseColor.BLACK)));// { Border = Rectangle.NO_BORDER };
                cell.BorderColor = BaseColor.LIGHT_GRAY;
                table2.AddCell(cell);

                c = new Chunk("Report Number", new Font(Font.FontFamily.TIMES_ROMAN, 12f, Font.BOLD, BaseColor.BLACK));
                cell = new PdfPCell(new Paragraph(c));// { Border = Rectangle.NO_BORDER, PaddingLeft = 15f };
                cell.BorderColor = BaseColor.LIGHT_GRAY;
                table2.AddCell(cell);

                cell = new PdfPCell(new Paragraph(testInfo.PumpReportNumber.ToString(), new Font(Font.FontFamily.HELVETICA, 12f, Font.ITALIC, BaseColor.BLACK)));// { Border = Rectangle.NO_BORDER };               
                cell.BorderColor = BaseColor.LIGHT_GRAY;
                table2.AddCell(cell);

                c = new Chunk("Job Card NO", new Font(Font.FontFamily.TIMES_ROMAN, 12f, Font.BOLD, BaseColor.BLACK));
                cell = new PdfPCell(new Paragraph(c));// { Border = Rectangle.NO_BORDER, PaddingLeft = 15f };
                cell.BorderColor = BaseColor.LIGHT_GRAY;
                table2.AddCell(cell);



                cell = new PdfPCell(new Paragraph(testInfo.PumpJobNumber.ToString(), new Font(Font.FontFamily.HELVETICA, 12f, Font.ITALIC, BaseColor.BLACK)));// { Border = Rectangle.NO_BORDER };                
                cell.BorderColor = BaseColor.LIGHT_GRAY;
                table2.AddCell(cell);

                c = new Chunk("SPG Serial NO", new Font(Font.FontFamily.TIMES_ROMAN, 12f, Font.BOLD, BaseColor.BLACK));
                cell = new PdfPCell(new Paragraph(c));// { Border = Rectangle.NO_BORDER, PaddingLeft = 15f };
                cell.BorderColor = BaseColor.LIGHT_GRAY;
                table2.AddCell(cell);

                //cell = new PdfPCell(new Paragraph(testInfo.PumpSPGSerialNo.ToString(), new Font(Font.FontFamily.HELVETICA, 12f, Font.ITALIC, BaseColor.BLACK)));// { Border = Rectangle.NO_BORDER };                
                cell.BorderColor = BaseColor.LIGHT_GRAY;
                table2.AddCell(cell);


                table2.DefaultCell.Border = Rectangle.BOX;
                table2.DefaultCell.BorderColor = BaseColor.LIGHT_GRAY;
                //ta1.AddCell(table);
                //ta1.WriteSelectedRows(0, -1, pdfDoc.LeftMargin, pdfDoc.PageSize.Height - 36, pdfWriter.DirectContent);
                cell = new PdfPCell(table2);
                cell.AddElement(table2);
                cell.Border = PdfPCell.BOX;
                cell.BorderWidth = 1.1f;
                cell.BorderColor = BaseColor.LIGHT_GRAY;
                table.AddCell(cell);

                //para1.Add(table);
                pdfDoc.Add(table);
            }
            // MessageBox.Show("Header Added");
        }


        /// <summary>
        /// This Method add the Testing Parameters to the Stroke Test report file.
        /// </summary>
        /// <param name="testInfo"></param>
        /// <param name="pdfDoc"></param>
        private void AddFootertoCertificate(Document pdfDoc)
        {
            //if(pdfDoc.PageCount)
            string website = string.Empty;
            string address = string.Empty;
            string refer = string.Empty;
            try
            {
                website = ConfigurationManager.AppSettings["WebSiteAddress"].ToString();
                address = ConfigurationManager.AppSettings["Address"].ToString();
                refer = ConfigurationManager.AppSettings["Ref"].ToString();
            }
            catch (Exception) { }
            PdfPTable table = new PdfPTable(4);
            table.WidthPercentage = 90;
            PdfPCell cell = new PdfPCell(new Paragraph(website, new Font(Font.FontFamily.COURIER, 14f, Font.BOLD, BaseColor.BLACK))) { Border = Rectangle.NO_BORDER, PaddingTop = 5, PaddingBottom = 5 };
            cell.Colspan = 4;
            cell.HorizontalAlignment = Element.ALIGN_CENTER;
            cell.VerticalAlignment = Element.ALIGN_CENTER;
            table.AddCell(cell);

            cell = new PdfPCell(new Paragraph(address, new Font(Font.FontFamily.TIMES_ROMAN, 10f, Font.NORMAL, BaseColor.BLACK))) { Border = Rectangle.NO_BORDER, PaddingTop = 2, PaddingBottom = 2 };
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


            PdfPCell cell1 = new PdfPCell(table) { Border = Rectangle.TOP_BORDER };
            cell1.AddElement(table);
            PdfPTable table1 = new PdfPTable(1);
            table1.WidthPercentage = 90;
            table1.AddCell(cell1);
            pdfDoc.Add(table1);

        }



        /// <summary>
        /// This Methos generates the image from the SeriesCollection and add this image to the PDF file.
        /// </summary>
        /// <param name="series"></param>
        /// <param name="labels"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private bool GenerateImageFromGraph(ObservableCollection<SeriesCollection> series, ObservableCollection<List<string>> labels, string fileName, TestType testType, int columnCount, PumpTestInformation pumpTestInfo)
        {
            Brush[] graphStrokes = new Brush[] { Brushes.DarkOrange, Brushes.DarkGreen, Brushes.DarkBlue, Brushes.Brown, Brushes.Red, Brushes.DarkCyan, Brushes.DarkMagenta, Brushes.DarkOliveGreen, Brushes.DarkOrange, Brushes.DarkSalmon };

            string xAxixName = pumpTestInfo.SelectedXaxis;
            Axis yAxix;


            int heigth = 0;
            int width = 0;
            if (columnCount == 2)
            {
                width = 550;
                heigth = 500;
            }
            else if (columnCount == 1)
            {
                //12/08/2018 chnages
                heigth = 400;
                width = 750;
            }
            else
            {
                width = 550;
                heigth = 500;
            }

            var currentChart = new LiveCharts.Wpf.CartesianChart  // Width = 550, Height = 500,
            {
                DisableAnimations = true,
                Width = width,
                Height = heigth,


                LegendLocation = LegendLocation.Bottom,
                //Series = new SeriesCollection
                //{
                //    series
                //}


                // FontSize=15
            };

            currentChart.Series = new SeriesCollection();
            currentChart.AxisY = new AxesCollection();



            foreach (var item in series)
            {
                yAxix = new Axis { Title = item[0].Title, Foreground = graphStrokes[series.IndexOf(item)]/*, MinValue = getMinValue(item[0].Values), MaxValue = getMaxValue(item[0].Values)*/ };
                currentChart.Series.Add(item[0]);
                currentChart.Series[series.IndexOf(item)].ScalesYAt = series.IndexOf(item);
                currentChart.AxisY.Add(yAxix);
            }

            currentChart.AxisX = new AxesCollection() { new Axis { Title = xAxixName, Foreground = Brushes.Black, Labels = labels[0] } };


            var viewbox = new Viewbox();
            viewbox.Child = currentChart;
            viewbox.Measure(currentChart.RenderSize);
            viewbox.Arrange(new Rect(new Point(0, 0), currentChart.RenderSize));
            currentChart.Update(true, true); //force chart redraw
            viewbox.UpdateLayout();
            //png file was created at the root directory.
            return (Helper.SaveToPng(currentChart, fileName));

        }

        private double getMinValue(IChartValues values)
        {
            List<double> val = new List<double>();
            foreach (var item in values)
            {
                val.Add(Convert.ToDouble(item));
            }
            double minVal = val.Min();
            return minVal - 200;
        }

        private double getMaxValue(IChartValues values)
        {
            List<double> val = new List<double>();
            foreach (var item in values)
            {
                val.Add(Convert.ToDouble(item));
            }
            double maxVal = val.Max();
            return maxVal + 200;
        }






        #endregion

        #region CSV Data File related operations

        /// <summary>
        /// Generates a .CSV file with observed values in a predefined location.
        /// </summary>
        /// <param name="testType"></param>
        /// <param name="testInfo"></param>
        /// <param name="noofCyclesCompleted"></param>
        /// <param name="lineSeriesCollection"></param>
        /// <param name="labelCollection"></param>
        internal void GenerateCSVFile(TestType testType, dynamic testInfo, ObservableCollection<LineSeries> lineSeriesCollection, ObservableCollection<List<string>> labelCollection, bool createNew = false, string noofCyclesCompleted = null)
        {
            try
            {
                //dynamic testInformation = Helper.GetTestObject(testType, testInfo);
                dynamic testInformation = (PumpTestInformation)testInfo;
                List<string> stringBuilder1 = new List<string>();
                List<string> stringBuilder2 = new List<string>();
                StringBuilder finalstring = new StringBuilder();
                StringBuilder dataString = new StringBuilder();
                List<string> datacollectionList = new List<string>();
                finalstring.AppendLine(string.Format("CustomerName,{0}", testInformation.EqipCustomerName));
                finalstring.AppendLine(string.Format("JobNumber,{0}", testInformation.PumpJobNumber));
                finalstring.AppendLine(string.Format("ReportNumber,{0}", testInformation.PumpReportName));
                finalstring.AppendLine(string.Format("SPGSerialNo,{0}", testInformation.PumpSPGSerialNo));
                finalstring.AppendLine(string.Format("Test Date,{0}", testInformation.TestDateTime));
                finalstring.AppendLine(string.Format("Equip Manufacturer,{0}", testInformation.EquipManufacturer));
                finalstring.AppendLine(string.Format("Equip Type,{0}", testInformation.EquipType));
                finalstring.AppendLine(string.Format("Equip ModelNo,{0}", testInformation.EquipModelNo));
                finalstring.AppendLine(string.Format("Equip SerialNo,{0}", testInformation.EquipSerialNo));
                finalstring.AppendLine(string.Format("Equip ControlType,{0}", testInformation.EquipControlType));
                finalstring.AppendLine(string.Format("Equip PumpType,{0}", testInformation.EquipPumpType));
                finalstring.AppendLine(string.Format("Test Manufacture,{0}", testInformation.TestManufacture));
                finalstring.AppendLine(string.Format("Test Type,{0}", testInformation.TestType));
                finalstring.AppendLine(string.Format("Test SerialNo,{0}", testInformation.TestSerialNo));
                finalstring.AppendLine(string.Format("Test Range,{0}", testInformation.TestRange));

                
                finalstring.AppendLine(string.Format("Test TestedBy,{0}", testInformation.TestedBy));
                finalstring.AppendLine(string.Format("Test WitnessedBy,{0}", testInformation.WitnessedBy));
                finalstring.AppendLine(string.Format("Test ApprovedBy,{0}", testInformation.OpprovedBy));

                finalstring.AppendLine(string.Format("Series Count,{0}", testInformation.SeriesCounts));
                finalstring.AppendLine(string.Format("Selected Xaxis,{0}", testInformation.SelectedXaxis));
                finalstring.AppendLine(string.Format("Table Para,{0}", string.Join(":", testInformation.TableParameterList)));

                for (int count = 0; count < lineSeriesCollection.Count; count++)
                {
                    List<string> labels = labelCollection[0];
                    List<string> xData = Helper.BuildStrig(labels, "");
                    List<string> yData = Helper.BuildStrig(lineSeriesCollection[count].Values, lineSeriesCollection[count].Title);
                    if (datacollectionList != null && datacollectionList.Count == 0)
                    {
                        if (xData != null)
                        {
                            for (int i = 0; i < Math.Min(xData.Count, yData.Count); i++)
                            {
                                datacollectionList.Add(string.Format("{0},{1}", xData[i], yData[i]));
                            }
                        }
                    }
                    else
                    {
                        for (int position = 0; position < datacollectionList.Count; position++)
                        {
                            datacollectionList[position] = string.Format("{0},{1},{2}", datacollectionList[position], xData[position], yData[position]);
                        }
                    }
                }
                for (int size = 0; size < datacollectionList.Count; size++)
                {
                    finalstring.AppendLine(datacollectionList[size]);
                }

                string csvFileName = string.Empty;
                if (testType == TestType.PumpTest)
                    csvFileName = string.Format(@"{0}\{3}\{2}\{1}__{2}.csv", ReportLocation, FilePrefix, testInformation.PumpReportNumber, testInformation.PumpJobNumber);
                if (!File.Exists(csvFileName))
                {
                    WriteDatatoFile(testInformation.PumpJobNumber, testInformation.PumpReportNumber, finalstring, csvFileName);
                }
                else
                {

                    int fileCount = 0;
                    while (File.Exists(csvFileName))
                    {
                        fileCount++;
                        if (fileCount == 1)
                            csvFileName = string.Concat(csvFileName.Substring(0, csvFileName.LastIndexOf('.')), '(', fileCount, ')', ".csv");
                        else
                            csvFileName = string.Concat(csvFileName.Substring(0, csvFileName.LastIndexOf('(')), '(', fileCount, ')', ".csv");
                    }
                    WriteDatatoFile(testInformation.PumpJobNumber, testInformation.PumpReportNumber, finalstring, csvFileName);

                }
            }
            catch (Exception ex)
            {
                ElpisServer.Addlogs("Report Tool", "Generate CSV", ex.Message, LogStatus.Error);

            }
        }







        /// <summary>
        /// Write the test information in CSV File.
        /// </summary>
        /// <param name="jobNumber"></param>
        /// <param name="finalstring"></param>
        /// <param name="csvFileName"></param>
        private void WriteDatatoFile(string jobNumber, string reportNumber, StringBuilder finalstring, string csvFileName)
        {
            try
            {
                Directory.CreateDirectory(string.Format(@"{0}\{2}\{1}", ReportLocation, reportNumber, jobNumber));
                File.AppendAllText(csvFileName, finalstring.ToString());// + Environment.NewLine);
                                                                        //MessageBox.Show("Data file created in following path:\n" + csvFileName, "SPG Report Tool", MessageBoxButton.OK);
            }
            catch (Exception ex)
            {
                MessageBox.Show("File Path:" + csvFileName + "\n" + ex.Message);
            }
        }

        #endregion
    }
}
