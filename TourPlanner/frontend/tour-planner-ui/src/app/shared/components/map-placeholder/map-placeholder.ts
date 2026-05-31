import {
  afterNextRender,
  Component,
  ElementRef,
  OnDestroy,
  effect,
  inject,
  input,
  viewChild,
} from '@angular/core';

import { MapFacadeService } from '../../../core/services/map-facade.service';

@Component({
  selector: 'app-map-placeholder',
  templateUrl: './map-placeholder.html',
})
export class MapPlaceholder implements OnDestroy {
  private readonly mapFacade = inject(MapFacadeService);

  readonly from = input.required<string>();
  readonly to = input.required<string>();
  readonly transportType = input.required<string>();
  readonly distance = input.required<number>();
  readonly estimatedTime = input.required<number>();

  readonly mapContainer = viewChild.required<ElementRef<HTMLDivElement>>('mapContainer');

  constructor() {
    afterNextRender(() => {
      this.mapFacade.initMap(this.mapContainer().nativeElement);
      this.updateMap();
    });

    effect(() => {
      this.updateMap();
    });
  }

  ngOnDestroy(): void {
    this.mapFacade.destroyMap();
  }

  private updateMap(): void {
    this.mapFacade.drawRoute(this.from(), this.to());
  }
}
