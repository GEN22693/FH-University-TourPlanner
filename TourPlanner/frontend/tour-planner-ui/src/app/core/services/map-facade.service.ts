import { Injectable } from '@angular/core';
import * as L from 'leaflet';

type Coordinate = [number, number];

interface GeocodeResult {
  lat: string;
  lon: string;
}

interface OsrmRouteResponse {
  routes?: Array<{
    geometry?: {
      type?: string;
      coordinates?: number[][];
    };
  }>;
}

export interface DrawRouteOptions {
  from: string;
  to: string;
  routeInformation: string;
}

@Injectable({
  providedIn: 'root',
})
export class MapFacadeService {
  private map: L.Map | null = null;
  private routeLayer: L.LayerGroup | null = null;
  private abortController: AbortController | null = null;

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

  async drawRoute(options: DrawRouteOptions): Promise<void> {
    if (!this.map || !this.routeLayer) {
      return;
    }

    this.abortController?.abort();
    this.abortController = new AbortController();

    this.routeLayer.clearLayers();

    const parsedBackendRoute = this.tryParseBackendRoute(options.routeInformation);

    if (parsedBackendRoute.length > 0) {
      this.drawRoutePoints(parsedBackendRoute, options.from, options.to);
      return;
    }

    try {
      const externalRoute = await this.loadRoadRouteFromLocations(
        options.from,
        options.to,
        this.abortController.signal,
      );

      if (externalRoute.length > 0) {
        this.drawRoutePoints(externalRoute, options.from, options.to);
        return;
      }
    } catch {
      // If external route loading fails, the map still shows a simple fallback line.
    }

    const fallbackStart = this.getFallbackCoordinate(options.from);
    const fallbackEnd = this.getFallbackCoordinate(options.to);
    const fallbackRoute = [
      fallbackStart,
      this.createMiddlePoint(fallbackStart, fallbackEnd),
      fallbackEnd,
    ];

    this.drawRoutePoints(fallbackRoute, options.from, options.to);
  }

  destroyMap(): void {
    this.abortController?.abort();
    this.abortController = null;

    this.map?.remove();
    this.map = null;
    this.routeLayer = null;
  }

  private drawRoutePoints(routePoints: Coordinate[], from: string, to: string): void {
    if (!this.map || !this.routeLayer || routePoints.length === 0) {
      return;
    }

    L.polyline(routePoints, {
      color: '#ffffff',
      weight: 11,
      opacity: 0.95,
      lineCap: 'round',
      lineJoin: 'round',
    }).addTo(this.routeLayer);

    L.polyline(routePoints, {
      color: '#1a73e8',
      weight: 7,
      opacity: 1,
      lineCap: 'round',
      lineJoin: 'round',
    }).addTo(this.routeLayer);

    L.circleMarker(routePoints[0], {
      radius: 9,
      color: '#ffffff',
      weight: 4,
      fillColor: '#202124',
      fillOpacity: 1,
    })
      .bindPopup(`Start: ${from}`)
      .addTo(this.routeLayer);

    L.circleMarker(routePoints[routePoints.length - 1], {
      radius: 10,
      color: '#ffffff',
      weight: 4,
      fillColor: '#1a73e8',
      fillOpacity: 1,
    })
      .bindPopup(`Destination: ${to}`)
      .addTo(this.routeLayer);

    const bounds = L.latLngBounds(routePoints);

    this.map.fitBounds(bounds, {
      padding: [40, 40],
      maxZoom: 14,
    });
  }

  private async loadRoadRouteFromLocations(
    from: string,
    to: string,
    signal: AbortSignal,
  ): Promise<Coordinate[]> {
    const start = await this.geocodeLocation(from, signal);
    const end = await this.geocodeLocation(to, signal);

    if (!start || !end) {
      return [];
    }

    const url =
      `https://router.project-osrm.org/route/v1/driving/` +
      `${start[1]},${start[0]};${end[1]},${end[0]}` +
      `?overview=full&geometries=geojson`;

    const response = await fetch(url, { signal });

    if (!response.ok) {
      return [];
    }

    const data = (await response.json()) as OsrmRouteResponse;
    const coordinates = data.routes?.[0]?.geometry?.coordinates;

    if (!Array.isArray(coordinates)) {
      return [];
    }

    return coordinates
      .filter((coordinate): coordinate is [number, number] => {
        return (
          Array.isArray(coordinate) &&
          coordinate.length >= 2 &&
          typeof coordinate[0] === 'number' &&
          typeof coordinate[1] === 'number'
        );
      })
      .map(([longitude, latitude]) => [latitude, longitude]);
  }

  private async geocodeLocation(location: string, signal: AbortSignal): Promise<Coordinate | null> {
    const url =
      `https://nominatim.openstreetmap.org/search?format=json&limit=1&q=` +
      encodeURIComponent(location);

    const response = await fetch(url, { signal });

    if (!response.ok) {
      return null;
    }

    const results = (await response.json()) as GeocodeResult[];
    const firstResult = results[0];

    if (!firstResult) {
      return null;
    }

    const latitude = Number(firstResult.lat);
    const longitude = Number(firstResult.lon);

    if (!Number.isFinite(latitude) || !Number.isFinite(longitude)) {
      return null;
    }

    return [latitude, longitude];
  }

  private tryParseBackendRoute(routeInformation: string): Coordinate[] {
    if (!routeInformation.trim()) {
      return [];
    }

    try {
      const parsedRouteInformation = JSON.parse(routeInformation) as unknown;
      const coordinates = this.extractCoordinates(parsedRouteInformation);

      if (!coordinates) {
        return [];
      }

      return coordinates
        .filter((coordinate): coordinate is [number, number] => {
          return (
            Array.isArray(coordinate) &&
            coordinate.length >= 2 &&
            typeof coordinate[0] === 'number' &&
            typeof coordinate[1] === 'number'
          );
        })
        .map(([longitude, latitude]) => [latitude, longitude]);
    } catch {
      return [];
    }
  }

  private extractCoordinates(value: unknown): unknown[] | null {
    if (Array.isArray(value)) {
      return value;
    }

    if (!this.isObject(value)) {
      return null;
    }

    const type = value['type'];

    if (type === 'LineString' && Array.isArray(value['coordinates'])) {
      return value['coordinates'];
    }

    if (type === 'Feature' && this.isObject(value['geometry'])) {
      return this.extractCoordinates(value['geometry']);
    }

    if (type === 'FeatureCollection' && Array.isArray(value['features'])) {
      const firstFeature = value['features'][0];

      return this.extractCoordinates(firstFeature);
    }

    if (Array.isArray(value['coordinates'])) {
      return value['coordinates'];
    }

    if (Array.isArray(value['features'])) {
      const firstFeature = value['features'][0];

      return this.extractCoordinates(firstFeature);
    }

    if (this.isObject(value['geometry'])) {
      return this.extractCoordinates(value['geometry']);
    }

    return null;
  }

  private isObject(value: unknown): value is Record<string, unknown> {
    return typeof value === 'object' && value !== null;
  }

  private getFallbackCoordinate(location: string): Coordinate {
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

    if (normalizedLocation.includes('klagenfurt')) {
      return [46.6247, 14.3053];
    }

    if (normalizedLocation.includes('innsbruck')) {
      return [47.2692, 11.4041];
    }

    return [48.2082, 16.3738];
  }

  private createMiddlePoint(start: Coordinate, end: Coordinate): Coordinate {
    const middleLat = (start[0] + end[0]) / 2 + 0.035;
    const middleLng = (start[1] + end[1]) / 2 - 0.025;

    return [middleLat, middleLng];
  }
}
