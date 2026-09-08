# Quinntyne Brown Studio business cards

One double-sided landscape design: a charcoal front with a white Georgia wordmark,
and a white back with the website and a small olive accent. The website follows
[the domain strategy](../../deploy/domain-strategy.md); its inclusion does not verify
that the site is live.

## Take to the print shop

- **card-bleed.pdf**: preferred two-page vector master, front then back, with embedded
  Georgia and Arial subsets. Each page is 3.75 × 2.25 inches including bleed.
- **card-crop-marks.pdf**: the same artwork with crop marks outside the bleed;
  larger 4.167 × 2.667 inch media. TrimBox and BleedBox identify the intended cut.
- **card-front.svg / card-back.svg**: vector artwork with lettering converted to
  paths, so no installed fonts are required. Includes bleed; trim 0.125 inch from
  every edge. No crop marks.
- **card-front-300dpi.png / card-back-300dpi.png**: 1125 × 675 pixel RGB exports,
  with 300 DPI metadata and bleed. No crop marks. Prefer PDF for sharp vector text.
- **print-sheet-letter.pdf / print-sheet-a4.pdf**: eight cards per sheet, two pages
  for front and back, with cutting guides and separate bleed for every card.

Finished size: **3.5 × 2 inches (88.9 × 50.8 mm)**. Bleed: **0.125 inch (3.175 mm)**
on each edge. Essential content stays at least 0.125 inch inside the cut.

Print PDFs at **100% / Actual size**, with no Fit, Shrink, or automatic page scaling.
For imposed sheets, use portrait paper and **double-sided, flip on long edge**.
Page 1 is the front, page 2 the back; do not mirror or rotate artwork manually.
Run one duplex proof before the batch, check registration against the guides,
and confirm the trimmed dimensions with a ruler. Feed alignment varies by printer.
For manual duplexing, follow that printer's paper-feed instructions.

Artwork is RGB (PDF DeviceRGB; SVG/PNG use the matching RGB values), with no
printer-specific ICC conversion and no claim of PDF/X compliance. Brand colors:
charcoal #242620, white #ffffff, olive #5b654c. Ask the shop to use its stock/press
profile when converting and proof the dark fill. Select the shop's required master
variant; use its own imposition workflow when requested.

## Editable source and verification

`generate.py` is the editable source for all formats. It reads the authoritative
design-system color tokens. Fonts remain in the Windows installation rather than
being redistributed. Regenerate from the repository root on Windows:

```powershell
python -m pip install reportlab pypdf pymupdf fonttools pillow
python docs/business-cards/generate.py
```

The generator checks page count, extracted wording, embedded TrueType subsets,
master trim/bleed dimensions, and PNG pixel dimensions. `verification.json` records
PDF results. `previews/` contains rendered PDF pages for layout review, not print
masters. Front/back sheet positions are mirrored across the long-edge duplex axis.
No application code or public interfaces change.
