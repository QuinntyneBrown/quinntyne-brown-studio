import { AvailabilityService } from '@qbs/api';
import { TORONTO_TIME_SERVICE } from '@qbs/api';
import { TorontoTimeService } from './time/toronto-time-service';
import { CATALOG_SERVICE, CatalogService } from '@qbs/api';
import {
  EquipmentService,
  VendorService,
  PhotographerService,
  SessionService,
  PromotionService,
  PrintOptionService,
  PublicGalleryService,
  StudioService,
  RateService,
  DiscountService,
  ContentService,
  ScheduleService,
  ClientGalleryService,
  AlbumService,
  PrintRequestService,
  PhotoService,
  RetentionService,
  AnalysisService,
  UploadService,
} from '@qbs/api';
import { ACCOUNT_SERVICE } from './account.token';
import { AccountService } from './account-service';
import { AuthService } from '@qbs/api';
import { quoteProvider } from './quote-provider';
import { Provider } from '@angular/core';
import {
  STUDIO_CLIENT,
  StudioClient,
  ALBUM_SERVICE,
  ANALYSIS_SERVICE,
  AVAILABILITY_SERVICE,
  AUTH_SERVICE,
  CLIENT_GALLERY_SERVICE,
  CONTENT_SERVICE,
  DISCOUNT_SERVICE,
  EQUIPMENT_SERVICE,
  PHOTOGRAPHER_SERVICE,
  PHOTO_SERVICE,
  PRINT_OPTION_SERVICE,
  PRINT_REQUEST_SERVICE,
  PROMOTION_SERVICE,
  PUBLIC_GALLERY_SERVICE,
  RATE_SERVICE,
  RETENTION_SERVICE,
  SCHEDULE_SERVICE,
  SESSION_SERVICE,
  STUDIO_SERVICE,
  UPLOAD_SERVICE,
  VENDOR_SERVICE,
} from '@qbs/api';
export function studioProviders(): Provider[] {
  return [
    { provide: STUDIO_CLIENT, useClass: StudioClient },
    quoteProvider(),
    { provide: AVAILABILITY_SERVICE, useClass: AvailabilityService },
    { provide: TORONTO_TIME_SERVICE, useClass: TorontoTimeService },
    { provide: CATALOG_SERVICE, useClass: CatalogService },
    { provide: ACCOUNT_SERVICE, useClass: AccountService },
    { provide: AUTH_SERVICE, useClass: AuthService },
    { provide: EQUIPMENT_SERVICE, useClass: EquipmentService },
    { provide: VENDOR_SERVICE, useClass: VendorService },
    { provide: PHOTOGRAPHER_SERVICE, useClass: PhotographerService },
    { provide: SESSION_SERVICE, useClass: SessionService },
    { provide: PROMOTION_SERVICE, useClass: PromotionService },
    { provide: PRINT_OPTION_SERVICE, useClass: PrintOptionService },
    { provide: PUBLIC_GALLERY_SERVICE, useClass: PublicGalleryService },
    { provide: STUDIO_SERVICE, useClass: StudioService },
    { provide: RATE_SERVICE, useClass: RateService },
    { provide: DISCOUNT_SERVICE, useClass: DiscountService },
    { provide: CONTENT_SERVICE, useClass: ContentService },
    { provide: SCHEDULE_SERVICE, useClass: ScheduleService },
    { provide: CLIENT_GALLERY_SERVICE, useClass: ClientGalleryService },
    { provide: ALBUM_SERVICE, useClass: AlbumService },
    { provide: PRINT_REQUEST_SERVICE, useClass: PrintRequestService },
    { provide: PHOTO_SERVICE, useClass: PhotoService },
    { provide: RETENTION_SERVICE, useClass: RetentionService },
    { provide: ANALYSIS_SERVICE, useClass: AnalysisService },
    { provide: UPLOAD_SERVICE, useClass: UploadService },
  ];
}
