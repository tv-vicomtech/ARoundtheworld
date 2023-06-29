using System;
using UnityEngine;

/**
 * Class that handles sphere point to globe location
 */
public class LocationService : MonoBehaviour
{
    public Action<GameObject, Vector3, Vector2> OnClick;

    public static double DistanceBetweenPoints(double lat1, double lon1, double lat2, double lon2, char unit = 'K')
    {
        double rlat1 = Math.PI * lat1 / 180;
        double rlat2 = Math.PI * lat2 / 180;
        double theta = lon1 - lon2;
        double rtheta = Math.PI * theta / 180;
        double dist =
            Math.Sin(rlat1) * Math.Sin(rlat2) + Math.Cos(rlat1) *
            Math.Cos(rlat2) * Math.Cos(rtheta);
        dist = Math.Acos(dist);
        dist = dist * 180 / Math.PI;
        dist = dist * 60 * 1.1515;

        switch (unit)
        {
            case 'K': // Kilometers
                return dist * 1.609344;
            case 'N': // Nautical Miles 
                return dist * 0.8684;
            case 'M': // Miles
                return dist;
        }

        return dist;
    }

    public bool IsLocation(Vector2 lngLat1, Vector2 lngLat2, double threshold)
    {
        return DistanceBetweenPoints(lngLat1.y, lngLat1.x, lngLat2.y, lngLat1.x) < threshold;
    }

    public Vector2 ToSphericalCoordinates(Vector3 position)
    {
        // Convert to a unit vector so our y coordinate is in the range -1...1.
        position = position.normalized;

        // The vertical coordinate (y) varies as the sine of latitude, not the cosine.
        float lat = Mathf.Asin(position.y) * Mathf.Rad2Deg;

        // Use the 2-argument arctangent, which will correctly handle all four quadrants.
        float lon = Mathf.Atan2(position.x, position.z) * Mathf.Rad2Deg;

        // I usually put longitude first because I associate vector.x with "horizontal."
        return new Vector2(lon, lat);
    }

    private Vector2 FixLocation(Vector2 coords)
    {
        return new Vector2(-1, 1) * coords + new Vector2(90, -4);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                // Transform into collider's local coordinate system.
                Vector3 offset = hit.collider.transform.InverseTransformPoint(hit.point);
                Vector2 coords = ToSphericalCoordinates(offset);

                // Get LatLng oordinates
                Vector2 lonLat = FixLocation(coords);
                OnClick?.Invoke(hit.collider.gameObject, hit.point, lonLat);
            }
        }
    }
}
