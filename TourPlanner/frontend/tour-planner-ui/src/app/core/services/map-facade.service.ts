import { Injectable } from '@angular/core';
import * as L from 'leaflet';

type Coordinate = [number, number];

@Injectable({
  providedIn: 'root',
})
export class MapFacadeService {
  private map: L.Map | null = null;
  private routeLayer: L.LayerGroup | null = null;

  initMap(container: HTMLElement): void {
    if (this.map) {
      return;
    }

    this.map = L.map(container, {
      zoomControl: true,
      attributionControl: true,
    });

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
      attribution: '© OpenStreetMap contributors',
    }).addTo(this.map);

    this.routeLayer = L.layerGroup().addTo(this.map);
    this.map.setView([48.2082, 16.3738], 12);
  }

  drawRoute(from: string, to: string): void {
    if (!this.map || !this.routeLayer) {
      return;
    }

    this.routeLayer.clearLayers();

    const start = this.getCoordinate(from);
    const end = this.getCoordinate(to);

    const routePoints: Coordinate[] = [start, this.createMiddlePoint(start, end), end];

    L.polyline(routePoints, {
      color: '#c28a43',
      weight: 5,
      opacity: 0.95,
      lineCap: 'round',
      lineJoin: 'round',
    }).addTo(this.routeLayer);

    L.circleMarker(start, {
      radius: 9,
      color: '#f2eee5',
      weight: 4,
      fillColor: '#151515',
      fillOpacity: 1,
    })
      .bindPopup(`Start: ${from}`)
      .addTo(this.routeLayer);

    L.circleMarker(end, {
      radius: 10,
      color: '#f2eee5',
      weight: 4,
      fillColor: '#c28a43',
      fillOpacity: 1,
    })
      .bindPopup(`Destination: ${to}`)
      .addTo(this.routeLayer);

    const bounds = L.latLngBounds(routePoints);
    this.map.fitBounds(bounds, {
      padding: [40, 40],
      maxZoom: 13,
    });
  }

  destroyMap(): void {
    this.map?.remove();
    this.map = null;
    this.routeLayer = null;
  }

  private getCoordinate(location: string): Coordinate {
    const normalizedLocation = location.toLowerCase().trim();

    if (normalizedLocation.includes('klosterneuburg')) {
      return [48.3052, 16.3252];
    }

    if (normalizedLocation.includes('tulln')) {
      return [48.3315, 16.056];
    }

    if (normalizedLocation.includes('st. pölten') || normalizedLocation.includes('sankt pölten')) {
      return [48.2047, 15.6256];
    }

    if (normalizedLocation.includes('graz')) {
      return [47.0707, 15.4395];
    }

    if (normalizedLocation.includes('linz')) {
      return [48.3069, 14.2858];
    }

    if (normalizedLocation.includes('salzburg')) {
      return [47.8095, 13.055];
    }

    return [48.2082, 16.3738];
  }

  private createMiddlePoint(start: Coordinate, end: Coordinate): Coordinate {
    const middleLat = (start[0] + end[0]) / 2 + 0.035;
    const middleLng = (start[1] + end[1]) / 2 - 0.025;

    return [middleLat, middleLng];
  }
}
