import { InjectionToken } from '@angular/core';
import { IEquipmentService } from './equipment.contract';
export const EQUIPMENT_SERVICE = new InjectionToken<IEquipmentService>('EQUIPMENT_SERVICE');
