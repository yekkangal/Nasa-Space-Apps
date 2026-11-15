// SI-assisted: written with AI tools (LLM-generated code). See docs/SI_DISCLOSURE.md.
// Disclosure: docs/SI_DISCLOSURE.md
namespace MissionArchitect
{
    /// <summary>Asteroid mission references for the in-game Sources spread and docs/REFERENCES.md.</summary>
    public static class AsteroidSources
    {
        public const string Credit =
            "Data credit: NASA, NASA/JPL-Caltech, NASA Goddard, University of Arizona, " +
            "Southwest Research Institute where applicable. Values marked game are design assumptions, " +
            "not NASA data. This project is not affiliated with or endorsed by NASA.";

        public const string VerifyNote =
            "A human must verify every link before final submission.";

        public static readonly MissionSource[] All =
        {
            new MissionSource(1, "NASA, OSIRIS-REx Press Kit (August 2016).",
                "https://www.nasa.gov/wp-content/uploads/2016/06/osiris-rex_press_kit_0.pdf",
                "Dry mass 880 kg, wet mass 2,110 kg, sample return capsule 46 kg, solar power 1,226-3,000 W, " +
                "3.35 m sampling arm, Atlas V 411 class."),
            new MissionSource(2, "NASA, OSIRIS-REx Fact Sheet.",
                "https://www.nasa.gov/wp-content/uploads/2016/06/osiris_rex_factsheet5-25.pdf",
                "Launch period; the spacecraft orbited the Sun for a year before using Earth's gravity assist."),
            new MissionSource(3, "NASA, Bennu Facts.",
                "https://science.nasa.gov/solar-system/asteroids/101955-bennu/facts/",
                "Bennu about half a kilometre wide, 4.3 h rotation, 1.2 year orbit, mean distance from the Sun " +
                "about 168 million km, closest Earth approach about 299,000 km every ~6 years, boulders up to " +
                "22 m, returned sample 121.6 g."),
            new MissionSource(4, "NASA, OSIRIS-REx mission page (TAG, sample timeline).",
                "https://science.nasa.gov/mission/osiris-rex/",
                "TAG on 20 October 2020; requirement at least 60 g; 70.3 g found outside the sampler head."),
            new MissionSource(5, "Beshore et al., OSIRIS-REx: Sample Return from Asteroid (101955) Bennu, IEEE Aerospace 2015.",
                "https://ael.gsfc.nasa.gov/sed/content/uploadFiles/publication_files/Beshore%20et%20al%20IEEE%202015.pdf",
                "Mission cost roughly 800 M$ without launch vehicle; Atlas V about 183.5 M$; cost cap about 850 M$ plus launch."),
            new MissionSource(6, "NASA, OSIRIS-REx mission timeline (arrival, orbit, return).",
                "https://science.nasa.gov/mission/osiris-rex/",
                "Arrival 3 December 2018 at ~19 km; orbit at ~1.75 km on 31 December 2018; departure 10 May 2021; " +
                "capsule return 24 September 2023.")
        };
    }
}
