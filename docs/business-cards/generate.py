"""Regenerate the business card package using licensed Windows Georgia and Arial."""
from pathlib import Path
from io import BytesIO
import json
import re
import fitz
from PIL import Image
from fontTools.ttLib import TTFont as OutlineFont
from fontTools.pens.svgPathPen import SVGPathPen
from reportlab.pdfgen import canvas
from reportlab.pdfbase import pdfmetrics
from reportlab.pdfbase.ttfonts import TTFont
from reportlab.lib.colors import HexColor
from pypdf import PdfReader, PdfWriter
from pypdf.generic import RectangleObject

ROOT = Path(__file__).resolve().parent
TOKENS = (ROOT.parents[1] / 'design-system/assets/tokens.css').read_text()
COLORS = {name: re.search(r'--' + name + r':\s*(#[0-9a-f]+)', TOKENS)[1]
          for name in ('ink', 'paper', 'accent')}
FONTS = {'Georgia': Path('C:/Windows/Fonts/georgia.ttf'),
         'Arial': Path('C:/Windows/Fonts/arial.ttf')}
for name, path in FONTS.items():
    pdfmetrics.registerFont(TTFont(name, str(path)))
W, H, B = 252, 144, 9
WEBSITE = 'quinntynebrown.studio'


def elements(side):
    if side == 'front':
        return [('Quinntyne Brown', 'Georgia', 23, 79, 'paper'),
                ('S T U D I O', 'Arial', 9, 54, 'paper')]
    return [(WEBSITE, 'Arial', 12, 67, 'ink')]


def artwork(c, side, x, y):
    c.saveState()
    c.translate(x, y)
    c.setFillColor(HexColor(COLORS['ink' if side == 'front' else 'paper']))
    c.rect(-B, -B, W + 2 * B, H + 2 * B, fill=1, stroke=0)
    if side == 'back':
        c.setFillColor(HexColor(COLORS['accent']))
        c.rect(W / 2 - 13, 93, 26, 1.5, fill=1, stroke=0)
    for text, font, size, baseline, color in elements(side):
        c.setFillColor(HexColor(COLORS[color]))
        c.setFont(font, size)
        c.drawCentredString(W / 2, baseline, text)
    c.restoreState()


def marks(c, x, y):
    c.setStrokeColorRGB(0, 0, 0)
    c.setLineWidth(.25)
    for dx in (0, W):
        c.line(x + dx, y - B - 2, x + dx, y - B - 7)
        c.line(x + dx, y + H + B + 2, x + dx, y + H + B + 7)
    for dy in (0, H):
        c.line(x - B - 7, y + dy, x - B - 2, y + dy)
        c.line(x + W + B + 2, y + dy, x + W + B + 7, y + dy)


def single_pdf(filename, margin, crop):
    stream = BytesIO()
    c = canvas.Canvas(stream, pagesize=(W + margin * 2, H + margin * 2))
    c.setTitle('Quinntyne Brown Studio | Business card')
    for side in ('front', 'back'):
        artwork(c, side, margin, margin)
        if crop:
            marks(c, margin, margin)
        c.showPage()
    c.save()
    writer = PdfWriter()
    for page in PdfReader(stream).pages:
        page.trimbox = RectangleObject((margin, margin, margin + W, margin + H))
        page.bleedbox = RectangleObject((margin - B, margin - B, margin + W + B, margin + H + B))
        writer.add_page(page)
    with (ROOT / filename).open('wb') as output:
        writer.write(output)


def sheet(filename, width, height):
    c = canvas.Canvas(str(ROOT / filename), pagesize=(width, height))
    c.setTitle('Quinntyne Brown Studio | Duplex print sheet')
    gap = 36
    left, bottom = (width - 2 * W - gap) / 2, (height - 4 * H - 3 * gap) / 2
    for side in ('front', 'back'):
        for row in range(4):
            for col in range(2):
                # Mirror positions across the long-edge duplex axis.
                x = left + (col if side == 'front' else 1 - col) * (W + gap)
                y = bottom + row * (H + gap)
                artwork(c, side, x, y)
                marks(c, x, y)
        c.showPage()
    c.save()


def svg(side):
    out = [f'<svg xmlns="http://www.w3.org/2000/svg" width="3.75in" height="2.25in" viewBox="0 0 270 162">',
           f'<title>Quinntyne Brown Studio business card - {side}</title>',
           f'<rect width="270" height="162" fill="{COLORS["ink" if side == "front" else "paper"]}"/>']
    if side == 'back':
        out.append(f'<rect x="122" y="66.5" width="26" height="1.5" fill="{COLORS["accent"]}"/>')
    for text, name, size, baseline, color in elements(side):
        font = OutlineFont(FONTS[name])
        glyphs, cmap = font.getGlyphSet(), font.getBestCmap()
        scale = size / font['head'].unitsPerEm
        x = B + (W - pdfmetrics.stringWidth(text, name, size)) / 2
        for char in text:
            glyph = glyphs[cmap[ord(char)]]
            pen = SVGPathPen(glyphs)
            glyph.draw(pen)
            out.append(f'<path fill="{COLORS[color]}" transform="translate({x},{B + H - baseline}) scale({scale},{-scale})" d="{pen.getCommands()}"/>')
            x += glyph.width * scale
        font.close()
    out.append('</svg>')
    (ROOT / f'card-{side}.svg').write_text('\n'.join(out), encoding='utf-8')


def verify_and_render():
    report = {}
    preview_dir = ROOT / 'previews'
    preview_dir.mkdir(exist_ok=True)
    for path in ROOT.glob('*.pdf'):
        reader, doc = PdfReader(path), fitz.open(path)
        assert len(reader.pages) == 2
        for i, page in enumerate(reader.pages):
            expected = 'Quinntyne Brown' if i == 0 else WEBSITE
            assert expected in page.extract_text()
            for ref in page['/Resources']['/Font'].values():
                font = ref.get_object()
                if '/FontDescriptor' in font:
                    assert '/FontFile2' in font['/FontDescriptor']
            doc[i].get_pixmap(matrix=fitz.Matrix(1.5, 1.5)).save(preview_dir / f'{path.stem}-{i + 1}.png')
        if path.name.startswith('card-'):
            for page in reader.pages:
                assert tuple(float(v) for v in (page.trimbox.width, page.trimbox.height)) == (W, H)
                assert tuple(float(v) for v in (page.bleedbox.width, page.bleedbox.height)) == (270, 162)
        report[path.name] = {'pages': 2, 'page_size_points': list(doc[0].rect)[2:], 'text_and_geometry': 'passed'}
        doc.close()
    doc = fitz.open(ROOT / 'card-bleed.pdf')
    for i, side in enumerate(('front', 'back')):
        pix = doc[i].get_pixmap(dpi=300)
        img = Image.frombytes('RGB', (pix.width, pix.height), pix.samples)
        assert img.size == (1125, 675)
        img.save(ROOT / f'card-{side}-300dpi.png', dpi=(300, 300))
    doc.close()
    (ROOT / 'verification.json').write_text(json.dumps(report, indent=2) + '\n')


if __name__ == '__main__':
    single_pdf('card-bleed.pdf', B, False)
    single_pdf('card-crop-marks.pdf', 24, True)
    sheet('print-sheet-letter.pdf', 612, 792)
    sheet('print-sheet-a4.pdf', 595.2756, 841.8898)
    for side in ('front', 'back'):
        svg(side)
    verify_and_render()
    print('Generated and checked four PDFs, two outlined SVGs, and two 300 DPI PNGs.')
