import { InjectionToken } from '@angular/core';
import { IAnalysisApi } from './analysis.contract';
export const ANALYSIS_API = new InjectionToken<IAnalysisApi>('AnalysisApi');
