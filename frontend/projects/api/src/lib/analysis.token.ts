import { InjectionToken } from '@angular/core';
import { IAnalysisService } from './analysis.contract';
export const ANALYSIS_SERVICE = new InjectionToken<IAnalysisService>('ANALYSIS_SERVICE');
