using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using Qkmaxware.Measurement;
using System;
using System.IO;
using System.Drawing;

namespace Qkmaxware.Astro.Control.Platesolving {

/// <summary>
/// Wrapper for the Astronomy.Net platesolving
/// http://astrometry.net/doc/net/api.html
/// </summary>
public class AstronomyNet : IPlateSolver {
    public static readonly string DefaultApiUrl = "https://nova.astrometry.net/api/";

    private string apiUrl;
    private string sessionKey;

    public AstronomyNet() {
        this.apiUrl = DefaultApiUrl;
    }
    public AstronomyNet(string apiUrl) {
        if (!apiUrl.EndsWith("/"))
            apiUrl += "/";
        this.apiUrl = apiUrl;
    }

    private class AstronomyNetResponse {
        public string status {get; set;}
        public bool IsSuccess() => status != null && status == "success";
        public string message {get; set;}
    }

    private class LoginResult : AstronomyNetResponse{
        public string session {get; set;}
    }
    public async Task Login(string apiKey) {
        using (var client = new HttpClient())
        using (var content = new FormUrlEncodedContent(new Dictionary<string, string>{
            {"apikey", apiKey}
        })) {
            content.Headers.Clear();
            content.Headers.Add("Content-Type", "application/x-www-form-urlencoded");

            var request = await client.PostAsync(this.apiUrl + "login", content);
            var response = await request.Content.ReadAsStringAsync();
            var login = JsonSerializer.Deserialize<LoginResult>(response);
            if (login != null && login.IsSuccess()) {
                this.sessionKey = login.session;
            }
        }
    }
    public bool IsLoggedIn => !string.IsNullOrEmpty(this.sessionKey);

    private class UploadRequest {
        public bool IsPublic = false;
        public string publicly_visible {
            get => IsPublic ? "y" : "n";
            set => this.IsPublic = value == "y";
        }
        public string allow_commercial_use => "d";
        public string allow_modifications => "d";
        public double? center_ra = null;
        public double? center_dec = null;
        public double radius = 0;
        public string session {get; set;}
    }
    private class SubmissionReceipt : AstronomyNetResponse{
        public int subid {get; set;}
        public string hash {get; set;}
    }
    public bool TryBlindSolve(string pathToImage, out IPlateSolvingResult results) {
        return TrySolve(pathToImage, null, null, Angle.Degrees(180), out results);
    }

    public bool TrySolve(string pathToImage, Angle approxRa, Angle approxDec, Angle searchRadius, out IPlateSolvingResult results) {
        if (!IsLoggedIn)
            throw new UnauthorizedAccessException ("You have not logged into the Astronomy.Net servers yet. Use .Login() with your api key before making any requests.");
        
        using (var client = new HttpClient())
        using (var content = new MultipartContent()) 
        using (var stream = new FileStream(pathToImage, FileMode.Open)) {
            var json = new UploadRequest {
                IsPublic = false,
                session = this.sessionKey,
                radius = (double)searchRadius.TotalDegrees(),
                center_ra = approxRa != null ? (double?)((double)approxRa.TotalDegrees()) : null,
                center_dec = approxDec != null ? (double?)((double)approxDec.TotalDegrees()) : null,
            };
            var firstPart = new FormUrlEncodedContent(new Dictionary<string,string>{
                {"request-json", JsonSerializer.Serialize(json)}
            });
            firstPart.Headers.Clear();
            firstPart.Headers.Add("Content-Type", "text/plain");
            content.Add(firstPart);

            var secondPart = new StreamContent(stream);
            content.Add(secondPart);

            var request = client.PostAsync(this.apiUrl + "upload", content);
            request.Wait();
            var result = request.Result.Content.ReadAsStringAsync();
            result.Wait();
            var receipt = JsonSerializer.Deserialize<SubmissionReceipt>(result.Result);
            if (receipt != null && receipt.IsSuccess()) {
                var awaitingResults = awaitResults(sessionKey, receipt.subid, receipt.hash);
                awaitingResults.Wait();
                results = awaitingResults.Result;
                return results != null && results.WasPlateSolvingSuccessful;
            }
        }

        results = new AstronomyNetResults {
            WasPlateSolvingSuccessful = false,
            PlateSolvingError = new Exception()
        }; 
        return false;
    }

    public TimeSpan PollingInterval {get; set;} = TimeSpan.FromMilliseconds(500);

    private class JobStatus : AstronomyNetResponse {
        public string processing_started {get; set;}
        public string processing_finished {get; set;}
        public int user {get; set;}
        public int[] jobs {get; set;}
        public int[][] job_calibrations {get; set;}

        public bool IsStarted() => !(jobs == null || jobs.Length == 0);
        public bool IsComplete() => job_calibrations != null && job_calibrations.Length > 0;
    }
    private class JobCalibrationResults {
        public double ra {get; set;}
        public double dec {get; set;}
        public double width_arcsec {get; set;}
        public double height_arcsec {get; set;}
        public double radius {get; set;}
        public double pixscale {get; set;}
        public double orientation {get; set;}
        public double parity {get; set;}
    }
    private class JobResults : AstronomyNetResponse {
        public JobCalibrationResults calibration {get; set;}
    }
    private async Task<IPlateSolvingResult> awaitResults(string forSession, int forSubmissionId, string forFileHash) {
        var awaiting = true;
        JobStatus status;

        // Wait until the job is done
        do {
            // Send request to check on status
            using (var client = new HttpClient()) {
                var statusRequest = await client.GetAsync(this.apiUrl + "submissions/" + forSubmissionId);
                var statusResults = await statusRequest.Content.ReadAsStringAsync();

                status = JsonSerializer.Deserialize<JobStatus>(statusResults);
                if (status != null && status.IsComplete()) {
                    awaiting = false;
                    break;
                }
            }

            // Delay the next check so we aren't polling infinitely
            await Task.Delay(PollingInterval);
        } while (awaiting);

        // Get the results
        using (var client = new HttpClient()) {
            // http://nova.astrometry.net/api/jobs/JOBID/calibration/
            var statusRequest = await client.GetAsync(this.apiUrl + "jobs/" + status.jobs[status.jobs.Length - 1] + "info");
            var statusResults = await statusRequest.Content.ReadAsStringAsync();

            var results = JsonSerializer.Deserialize<JobResults>(statusResults);
            if (results == null || !results.IsSuccess()) {
                return new AstronomyNetResults {
                    WasPlateSolvingSuccessful = false,
                    PlateSolvingError = new Exception(results?.message ?? "No results")
                };
            }

            return new AstronomyNetResults {
                WasPlateSolvingSuccessful = true,
                RightAscension = Angle.Degrees(results.calibration.ra),
                Declination = Angle.Degrees(results.calibration.dec),
            };
        }
    }
}

/// <summary>
/// Plate solving results from Astronomy.Net
/// </summary>
public class AstronomyNetResults : IPlateSolvingResult {
    public bool WasPlateSolvingSuccessful {get; internal set;}

    public Exception PlateSolvingError {get; internal set;}

    public Angle RightAscension {get; internal set;}

    public Angle Declination {get; internal set;}

}

}