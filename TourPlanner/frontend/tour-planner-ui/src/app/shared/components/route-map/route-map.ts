import {
  afterNextRender,
  Component,
  ElementRef,
  OnDestroy,
  effect,
  inject,
  input,
  signal,
  viewChild,
} from '@angular/core';

import { MapFacadeService } from '../../../core/services/map-facade.service';

@Component({
  selector: 'app-route-map',
  templateUrl: './route-map.html',
})
export class RouteMap implements OnDestroy {
  private readonly mapFacade = inject(MapFacadeService);

  readonly from = input.required<string>();
  readonly to = input.required<string>();
  readonly transportType = input.required<string>();
  readonly distance = input.required<number>();
  readonly estimatedTime = input.required<string>();
  readonly routeInformation = input.required<string>();

  readonly mapContainer = viewChild.required<ElementRef<HTMLDivElement>>('mapContainer');
  readonly isMapReady = signal(false);

  constructor() {
    afterNextRender(() => {
      this.mapFacade.initMap(this.mapContainer().nativeElement);
      this.isMapReady.set(true);
    });

    effect(() => {
      if (!this.isMapReady()) {
        return;
      }

      void this.mapFacade.drawRoute({
        from: this.from(),
        to: this.to(),
        routeInformation: this.routeInformation(),
      });
    });
  }

  ngOnDestroy(): void {
    this.mapFacade.destroyMap();
  }

  formatTourDistance(distanceInMeters: number): string {
    const distanceInKilometers = distanceInMeters / 1000;

    return `${Math.round(distanceInKilometers * 10) / 10} km`;
  }

  formatDuration(timeSpan: string): string {
    const totalMinutes = this.timeSpanToMinutes(timeSpan);

    if (totalMinutes <= 0) {
      return '0 min';
    }

    const hours = Math.floor(totalMinutes / 60);
    const minutes = totalMinutes % 60;

    if (hours === 0) {
      return `${minutes} min`;
    }

    if (minutes === 0) {
      return `${hours} h`;
    }

    return `${hours} h ${minutes} min`;
  }

  private timeSpanToMinutes(timeSpan: string): number {
    if (!timeSpan) {
      return 0;
    }

    const parts = timeSpan.split(':');

    if (parts.length !== 3) {
      return 0;
    }

    const hours = Number(parts[0]) || 0;
    const minutes = Number(parts[1]) || 0;
    const seconds = Number.parseFloat(parts[2]) || 0;

    return hours * 60 + minutes + Math.round(seconds / 60);
  }
}
