import { IAvailabilityService, AVAILABILITY_SERVICE } from '@qbs/api';
import { TORONTO_TIME_SERVICE } from '@qbs/api';
import { TorontoTimeService } from './time/toronto-time-service';
import { Provider } from '@angular/core';
import {
  controlledService,
  ICatalogService,
  IAuthService,
  IEquipmentService,
  IVendorService,
  IPhotographerService,
  ISessionService,
  IPromotionService,
  IPrintOptionService,
  IPublicGalleryService,
  IStudioService,
  IRateService,
  IDiscountService,
  IContentService,
  IScheduleService,
  IClientGalleryService,
  IAlbumService,
  IPrintRequestService,
  IPhotoService,
  IRetentionService,
  IAnalysisService,
  IUploadService,
  CATALOG_SERVICE,
  AUTH_SERVICE,
  EQUIPMENT_SERVICE,
  VENDOR_SERVICE,
  PHOTOGRAPHER_SERVICE,
  SESSION_SERVICE,
  PROMOTION_SERVICE,
  PRINT_OPTION_SERVICE,
  PUBLIC_GALLERY_SERVICE,
  STUDIO_SERVICE,
  RATE_SERVICE,
  DISCOUNT_SERVICE,
  CONTENT_SERVICE,
  SCHEDULE_SERVICE,
  CLIENT_GALLERY_SERVICE,
  ALBUM_SERVICE,
  PRINT_REQUEST_SERVICE,
  PHOTO_SERVICE,
  RETENTION_SERVICE,
  ANALYSIS_SERVICE,
  UPLOAD_SERVICE,
} from '@qbs/api';
import { ACCOUNT_SERVICE } from './account.token';
import { AccountService } from './account-service';
import { quoteProvider } from './quote-provider';
export function studioProviders(): Provider[] {
  return [
    quoteProvider(),
    {
      provide: AVAILABILITY_SERVICE,
      useFactory: () => controlledService<IAvailabilityService>('availability'),
    },
    { provide: TORONTO_TIME_SERVICE, useClass: TorontoTimeService },
    { provide: CATALOG_SERVICE, useFactory: () => controlledService<ICatalogService>('catalog') },
    { provide: ACCOUNT_SERVICE, useClass: AccountService },
    { provide: AUTH_SERVICE, useFactory: () => controlledService<IAuthService>('auth') },
    {
      provide: EQUIPMENT_SERVICE,
      useFactory: () => controlledService<IEquipmentService>('equipment'),
    },
    { provide: VENDOR_SERVICE, useFactory: () => controlledService<IVendorService>('vendor') },
    {
      provide: PHOTOGRAPHER_SERVICE,
      useFactory: () => controlledService<IPhotographerService>('photographer'),
    },
    { provide: SESSION_SERVICE, useFactory: () => controlledService<ISessionService>('session') },
    {
      provide: PROMOTION_SERVICE,
      useFactory: () => controlledService<IPromotionService>('promotion'),
    },
    {
      provide: PRINT_OPTION_SERVICE,
      useFactory: () => controlledService<IPrintOptionService>('print-option'),
    },
    {
      provide: PUBLIC_GALLERY_SERVICE,
      useFactory: () => controlledService<IPublicGalleryService>('public-gallery'),
    },
    { provide: STUDIO_SERVICE, useFactory: () => controlledService<IStudioService>('studio') },
    { provide: RATE_SERVICE, useFactory: () => controlledService<IRateService>('rate') },
    {
      provide: DISCOUNT_SERVICE,
      useFactory: () => controlledService<IDiscountService>('discount'),
    },
    { provide: CONTENT_SERVICE, useFactory: () => controlledService<IContentService>('content') },
    {
      provide: SCHEDULE_SERVICE,
      useFactory: () => controlledService<IScheduleService>('schedule'),
    },
    {
      provide: CLIENT_GALLERY_SERVICE,
      useFactory: () => controlledService<IClientGalleryService>('client-gallery'),
    },
    { provide: ALBUM_SERVICE, useFactory: () => controlledService<IAlbumService>('album') },
    {
      provide: PRINT_REQUEST_SERVICE,
      useFactory: () => controlledService<IPrintRequestService>('print-request'),
    },
    { provide: PHOTO_SERVICE, useFactory: () => controlledService<IPhotoService>('photo') },
    {
      provide: RETENTION_SERVICE,
      useFactory: () => controlledService<IRetentionService>('retention'),
    },
    {
      provide: ANALYSIS_SERVICE,
      useFactory: () => controlledService<IAnalysisService>('analysis'),
    },
    { provide: UPLOAD_SERVICE, useFactory: () => controlledService<IUploadService>('upload') },
  ];
}
