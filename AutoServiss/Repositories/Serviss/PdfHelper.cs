using AutoServiss.Database;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Collections.Generic;

namespace AutoServiss.Repositories.Serviss
{
    public class PdfHelper
    {
        private readonly Font _virsrakstaFonts;
        private readonly Font _tekstaFonts;

        public PdfHelper()
        {
            var baseFont = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1257, BaseFont.EMBEDDED);
            _virsrakstaFonts = new Font(baseFont, 12.0f, Font.BOLD);
            _tekstaFonts = new Font(baseFont, 12.0f, Font.NORMAL);
        }

        public Font VirsrakstaFonts => _virsrakstaFonts;

        public Font TekstaFonts => _tekstaFonts;

        public PdfPCell TuksaRinda(int colspan)
        {
            return new PdfPCell()
            {
                Colspan = colspan,
                PaddingBottom = 20f,
                Border = 0
            };
        }

        public PdfPCell TableCellLeft(string content)
        {
            return new PdfPCell(new Phrase(content, _tekstaFonts))
            {
                HorizontalAlignment = Element.ALIGN_LEFT,
                PaddingLeft = 6f,
                PaddingBottom = 6f
            };
        }

        public PdfPCell TableCellCenter(string content)
        {
            return new PdfPCell(new Phrase(content, _tekstaFonts))
            {
                HorizontalAlignment = Element.ALIGN_CENTER,
                PaddingBottom = 6f
            };
        }

        public PdfPCell TableCellTotalTitle()
        {
            return new PdfPCell(new Phrase("Kopā: ", _virsrakstaFonts))
            {
                Colspan = 4,
                HorizontalAlignment = Element.ALIGN_RIGHT,
                PaddingBottom = 6f,
                Border = 0
            };
        }

        public PdfPCell TableCellTotalSum(decimal sum)
        {
            return new PdfPCell(new Phrase(sum.ToString("F2"), _tekstaFonts))
            {
                Colspan = 1,
                HorizontalAlignment = Element.ALIGN_CENTER,
                PaddingBottom = 6f
            };
        }

        public PdfPTable DefektuTabula(string virsraksts, List<Defekts> defekti)
        {
            var widths = new float[] { 5f, 95f };
            var table = new PdfPTable(2)
            {
                WidthPercentage = 100
            };
            table.SetWidths(widths);

            table.AddCell(TuksaRinda(2));

            var cell = new PdfPCell(new Phrase("#", _virsrakstaFonts))
            {
                HorizontalAlignment = Element.ALIGN_CENTER,
                PaddingBottom = 6f
            };
            table.AddCell(cell);

            cell = new PdfPCell(new Phrase(virsraksts, _virsrakstaFonts))
            {
                HorizontalAlignment = Element.ALIGN_CENTER,
                PaddingBottom = 6f
            };
            table.AddCell(cell);

            var counter = 1;
            foreach (var def in defekti)
            {
                cell = new PdfPCell(new Phrase(counter.ToString(), _tekstaFonts))
                {
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    PaddingBottom = 6f
                };
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(def.Nosaukums, _tekstaFonts))
                {
                    HorizontalAlignment = Element.ALIGN_LEFT,
                    PaddingLeft = 6f,
                    PaddingBottom = 6f
                };
                table.AddCell(cell);

                counter++;
            }

            return table;
        }

        public PdfPTable CenuTabulasHeader(string veids)
        {
            var table = new PdfPTable(5)
            {
                WidthPercentage = 100
            };
            var widths = new float[] { 5f, 55f, 10f, 15f, 15f };
            table.SetWidths(widths);

            table.AddCell(TuksaRinda(5));

            var cell = new PdfPCell(new Phrase("#", _virsrakstaFonts))
            {
                HorizontalAlignment = Element.ALIGN_CENTER,
                PaddingBottom = 6f
            };
            table.AddCell(cell);

            cell = new PdfPCell(new Phrase(veids, _virsrakstaFonts))
            {
                HorizontalAlignment = Element.ALIGN_CENTER,
                PaddingBottom = 6f
            };
            table.AddCell(cell);

            cell = new PdfPCell(new Phrase("Skaits", _virsrakstaFonts))
            {
                HorizontalAlignment = Element.ALIGN_CENTER,
                PaddingBottom = 6f
            };
            table.AddCell(cell);

            cell = new PdfPCell(new Phrase("Mērvienība", _virsrakstaFonts))
            {
                HorizontalAlignment = Element.ALIGN_CENTER,
                PaddingBottom = 6f
            };
            table.AddCell(cell);

            cell = new PdfPCell(new Phrase("Cena", _virsrakstaFonts))
            {
                HorizontalAlignment = Element.ALIGN_CENTER,
                PaddingBottom = 6f
            };
            table.AddCell(cell);

            return table;
        }
    }
}