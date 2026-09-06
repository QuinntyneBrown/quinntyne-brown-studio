# Image storage strategy

Strategy and post-session workflow for a Nikon Z9 shooting about 2,000 RAW
frames a month. Azure is the cloud target. Prices are approximate list prices
(East US, locally redundant storage) at the time of writing; confirm on the
[Azure pricing calculator](https://azure.microsoft.com/pricing/calculator/)
before committing.

## Volume

| RAW format on the Z9 | Per frame | 2,000 frames / month | Per year |
| --- | --- | --- | --- |
| Lossless compressed (14-bit) | ~55 MB | ~110 GB | ~1.3 TB |
| High Efficiency RAW* (HE*) | ~33 MB | ~66 GB | ~0.8 TB |
| High Efficiency RAW (HE) | ~22 MB | ~44 GB | ~0.5 TB |

Add roughly 10–20 GB a month for edited exports and client deliverables.

**Recommendation:** shoot HE*. It is visually indistinguishable from lossless
for delivery work, is supported by Lightroom, Capture One and NX Studio, and
cuts every number below by about 40%. The tables use lossless as the worst
case so the plan still holds if you keep shooting it.

Five-year footprint at lossless: about 6.6 TB of RAW plus ~1 TB of exports.

## Decision: both, in defined roles

Neither cloud alone nor drives alone is the right answer.

- **Cloud only** is too slow to cull and edit from, and pulling a full archive
  back out of Azure costs more than the drives it would replace.
- **Drives only** die in the same fire, flood, or burglary as the studio.

Use the 3-2-1 rule: three copies, two media, one off-site.

| Copy | Where | Role |
| --- | --- | --- |
| 1 | NVMe / SSD working drive in the edit machine | Current jobs only, wiped after delivery |
| 2 | Two-bay NAS at the studio, mirrored drives | Primary library, every job, fast local access |
| 3 | Azure Blob Storage | Off-site copy, tiered to Archive after delivery |

## Cost breakdown

### Local hardware (one-time, five-year horizon)

| Item | Approx. cost |
| --- | --- |
| Two-bay NAS (e.g. Synology DS224+) | $300 |
| 2 × 12 TB NAS drives, mirrored (~11 TB usable) | $460 |
| 2 TB NVMe/SSD working drive | $130 |
| Second CFexpress Type B card (325 GB) so both slots record | $200 |
| **Total** | **~$1,100** |

Power and one replacement drive over five years add roughly $150. About
**$20 a month** amortized.

### Azure Blob Storage (per TB per month)

| Tier | Storage | Minimum retention | Notes |
| --- | --- | --- | --- |
| Hot | ~$18 | none | Client gallery deliverables served by the platform |
| Cool | ~$10 | 30 days | RAW for jobs in progress |
| Cold | ~$3.60 | 90 days | Optional middle step; skip it to keep the policy simple |
| Archive | ~$1 | 180 days | Delivered jobs. Rehydration takes up to 15 hours |

Extra charges that matter: Archive rehydration ~$0.02 per GB, and egress
~$0.09 per GB after the first 100 GB a month. Uploads are free.

### Azure monthly bill under the recommended policy

RAW sits in Cool for 90 days after the shoot, then moves to Archive. Exports
sit in Hot while the gallery is live, then Cool.

| Point in time | Cool (last 90 days) | Archive (older RAW) | Hot/Cool exports | Monthly |
| --- | --- | --- | --- | --- |
| End of year 1 | 0.33 TB → $3.30 | 1.0 TB → $1.00 | 0.2 TB → $3 | **~$7** |
| End of year 3 | 0.33 TB → $3.30 | 3.6 TB → $3.60 | 0.6 TB → $7 | **~$14** |
| End of year 5 | 0.33 TB → $3.30 | 6.3 TB → $6.30 | 1.0 TB → $11 | **~$21** |

Five-year Azure total is roughly **$800**. Keeping everything in Hot instead
would be about **$3,600**; everything in Archive from day one about **$200**
but unusable for working jobs.

### Disaster case

Restoring the whole five-year archive from Azure after losing the NAS costs
about $130 in rehydration plus $570 in egress, around **$700**, and takes a
day. That is the price of the insurance, not the routine cost, and is why the
NAS is the primary and Azure is the backup rather than the reverse.

### Five-year summary

| Option | Five-year cost | Verdict |
| --- | --- | --- |
| Drives only (NAS + rotating off-site USB drive) | ~$1,400 | Cheapest, but off-site copy depends on discipline |
| Azure only (Cool then Archive) | ~$800 + a fast desktop drive | Slow to work from, expensive to restore |
| **NAS + Azure (recommended)** | **~$2,000** | Fast locally, automatic off-site, survives a studio loss |

## Retention

- Keep every frame for 90 days after delivery.
- Then cull hard rejects (missed focus, blinks, test frames) before the
  lifecycle rule tiers the job to Archive. Typically 30–50% of a session, which
  halves the archive bill again. Keep everything if the contract requires it.
- Never delete from Azure by hand. Enable blob soft delete (14 days) so a
  mistake is reversible.

## Folder and naming convention

Identical on the working drive, the NAS, and the Azure container:

```text
photos/
  2026/
    2026-09-05_smith-wedding/
      raw/        Z9 files renamed 2026-09-05_smith-wedding_0001.NEF
      selects/    Lightroom catalog or Capture One session, edited picks
      exports/    Delivered JPEGs, sized as delivered
```

One Azure storage account, one container named `photos`, blob prefixes match
the folders. Locally redundant storage (LRS) is enough because the NAS is the
other copy.

## Post-session workflow

1. **Before the shoot.** Set the Z9 to record RAW to both CFexpress slots
   (Backup mode). Cards are the first mirror.
2. **Same day, before sleeping.** Ingest with Lightroom or Photo Mechanic from
   card one to the working drive using the naming convention. Turn on
   "Make a second copy to" pointed at the NAS. Apply copyright and client
   metadata on import.
3. **Verify.** Frame count on the card, the working drive, and the NAS must
   match. Only then reformat card two, and keep card one untouched until step 4
   completes.
4. **Overnight.** A scheduled `azcopy sync` on the studio Windows host pushes
   the NAS `photos/` folder to the Azure container in the Cool tier. Confirm
   the job log the next morning, then reformat card one.
5. **Cull and edit** from the working drive. Export deliverables to
   `exports/`, which the nightly sync also carries up. The client gallery
   serves from Blob Storage, so the upload is the delivery.
6. **On delivery.** Delete the job from the working drive. The NAS and Azure
   copies remain. Nothing else to do; the lifecycle rule handles tiering.
7. **90 days after delivery.** Cull rejects on the NAS, sync deletes to Azure,
   and let the lifecycle rule move the job to Archive.
8. **Quarterly.** Check NAS drive health (SMART), and restore one random job
   from Azure to prove the backup works.

## Azure setup checklist

- One storage account, Standard general-purpose v2, LRS, in the nearest region.
- Container `photos`, private access, soft delete on (14 days), versioning off.
- Lifecycle management rule: blobs under `photos/*/raw/` move to Archive 90
  days after last modification; blobs under `photos/*/exports/` move to Cool
  after 180 days.
- Upload with AzCopy using a SAS token scoped to the container, scheduled
  nightly through Windows Task Scheduler on the same host that runs the API
  and worker.
- Never grant the sync credential delete permission on Archive-tier blobs;
  deletions flow only through the reviewed cull step.
