// SI-assisted: written with AI tools (LLM-generated code). See docs/SI_DISCLOSURE.md.
// Disclosure: docs/SI_DISCLOSURE.md
namespace MissionArchitect
{
    public sealed class MissionSource
    {
        public int Id;
        public string Citation;
        public string Url;
        public string Supplies;

        public MissionSource(int id, string citation, string url, string supplies)
        {
            Id = id;
            Citation = citation;
            Url = url;
            Supplies = supplies;
        }
    }

    /// <summary>
    /// Shared NASA reference list for the in-game Sources spreads and docs/REFERENCES.md.
    /// A human must verify every link before final submission.
    /// </summary>
    public static class MissionSources
    {
        public const string Credit =
            "Data credit: NASA, NASA/JPL-Caltech, NASA Goddard, University of Arizona, " +
            "Southwest Research Institute where applicable. Values marked game are design assumptions, " +
            "not NASA data. This project is not affiliated with or endorsed by NASA.";

        public const string VerifyNote =
            "A human must verify every link before final submission.";

        public static readonly MissionSource[] All =
        {
            new MissionSource(1, "NASA NSSDC, Mars Fact Sheet.",
                "https://nssdc.gsfc.nasa.gov/planetary/factsheet/marsfact.html",
                "Mars gravity 3.71 m/s2, escape 5.03 km/s, radius 3396.2 km, GM 42,828 km3/s2, " +
                "solar irradiance 586-589 W/m2, 55-400 million km Earth-Mars distance, orbit radii."),
            new MissionSource(2, "NASA/JPL, Mars 2020 Perseverance quick facts.",
                "https://www.jpl.nasa.gov/news/press_kits/mars_2020/launch/quick_facts/",
                "Rover mass 1,025 kg, MMRTG ~110 W, Atlas V 541 class, 471 million km trip, " +
                "~2.4 B$ build+launch, ~300 M$ prime operations."),
            new MissionSource(3, "NASA/JPL, Mars 2020 launch period and window.",
                "https://www.jpl.nasa.gov/news/press_kits/mars_2020/launch/mission",
                "Launch window repeats about every 26 months; 1 Mars year = 687 Earth days."),
            new MissionSource(4, "NASA/JPL, Mars at a Glance (InSight press kit).",
                "https://www.jpl.nasa.gov/news/press_kits/insight/launch/facts/mars-at-a-glance",
                "Surface pressure is under 1/100 of Earth's."),
            new MissionSource(5, "NASA, Mars Relay Network.",
                "https://science.nasa.gov/mars/mars-relay-network-update/",
                "UHF relay up to 2 Mbps during roughly 8 minute orbiter passes."),
            new MissionSource(6, "NASA NTRS, Mars Perseverance Communications (2021).",
                "https://ntrs.nasa.gov/api/citations/20210010148/downloads/Mars%20Perseverance%20Communications%202021%20revised.pdf",
                "Direct-to-Earth X-band rates of 160-500 bps."),
            new MissionSource(7, "NASA JPL IPN Progress Report 42-246.",
                "https://ipnpr.jpl.nasa.gov/progress_report/42-246/42-246A.pdf",
                "Mars Relay Network typical rates 0.1-0.5 Mbps at greatest distance."),
            new MissionSource(8, "Zeitlin et al. / Hassler et al., Curiosity RAD measurements (2013-2014); SwRI release.",
                "https://www.swri.org/newsroom/press-releases/swri-led-team-calculates-the-radiation-exposure-associated-trip-mars",
                "Radiation dose equivalent 1.8 mSv/day in cruise, 0.67 mSv/day on the surface (surface ~0.37x cruise)."),
            new MissionSource(9, "NASA, Mars poster (average temperature).",
                "https://www.nasa.gov/pdf/173749main_Mars_Poster.pdf",
                "Average surface temperature about -81 F (about -63 C)."),
            new MissionSource(10, "NASA NSSDC, Moon Fact Sheet.",
                "https://nssdc.gsfc.nasa.gov/planetary/factsheet/moonfact.html",
                "Moon radius 1,737.4 km, GM 4,902.8 km3/s2, Earth-Moon distance 384,400 km, lunar gravity.")
        };

        public static MissionSource[] Mars
        {
            get { return new[] { All[0], All[1], All[2], All[3], All[4], All[5], All[6], All[7], All[8] }; }
        }

        public static MissionSource[] Moon
        {
            get { return new[] { All[9] }; }
        }
    }
}
