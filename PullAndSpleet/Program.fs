open System.Text
open System.Diagnostics
open System.Text.RegularExpressions
open System.IO
open FSharp.Data
open Amazon.Lambda.Core
open Amazon.S3.Model
open System

[<assembly: LambdaSerializer(typeof<Amazon.Lambda.Serialization.SystemTextJson.CamelCaseLambdaJsonSerializer>)>]
()

type SampletteResponse = JsonProvider<"""
[
{"id": 67953479, "url": "http://www.youtube.com/watch?v=6NvOLyBXoyE", "title": "Mary Prado - Deep Inside Me (Official Music Video)", "channel": "Cloud 9 Music", "channel_id": "UC_411v5XTGmfSfWD9Cpk-Zw", "published": "2011-05-19T14:32:37", "error": null, "views": 3499.0, "tags": "Mary,Prado,Deep,Inside,Me,Mark,van,Dale,vs,DJ,Team,Music,Old,School,Happy,Hardcore,Gabber,Official,Video,HD,Cloud,widescreen,dance music,rave,clubbing,new,your,techno,trance,oldskool,mix", "updated": "2022-06-01T16:58:31", "topics": null, "discogs": {"country": "Netherlands", "year": 1997.0, "genre": "Electronic", "style": "Trance|Euro House|Italodance", "label": "High Voltage (9)", "uri": "https://www.discogs.com/release/1529220-Mary-Prado-Deep-Inside-Of-Me", "updated": "2022-06-01T15:24:21", "master_id": 187625.0, "thumb": "https://i.discogs.com/UFvXGF1K8OXexx6qI-s7PMWBWKSpSkQWUbVdetfoIGo/rs:fit/g:sm/q:40/h:150/w:150/czM6Ly9kaXNjb2dz/LWRhdGFiYXNlLWlt/YWdlcy9SLTE1Mjky/MjAtMTIyNzEwNDY0/My5qcGVn.jpeg", "cover_image": "https://i.discogs.com/0rwd7I6htGQHN4rLZf0VsgE0_Me3LNbRaL_5-MdZpMU/rs:fit/g:sm/q:90/h:439/w:600/czM6Ly9kaXNjb2dz/LWRhdGFiYXNlLWlt/YWdlcy9SLTE1Mjky/MjAtMTIyNzEwNDY0/My5qcGVn.jpeg", "title": "Deep Inside Of Me", "format": "CD", "artist": "Mary Prado", "data_quality": "Needs Vote", "copyright": null, "phonographic_copyright": null, "tracklist": "Deep Inside Of Me (Radio Edit)|Deep Inside Of Me (Extended)|Deep Inside Of Me (Techno Remix)|Deep Inside Of Me (Trance Remix)", "release_id": 1529220.0, "rating_count": 22.0, "rating_average": 4.73, "genre_regex": "<Electronic>", "style_regex": "<Trance>|<Euro House>|<Italodance>", "country_regex": "<Netherlands>", "artist_title": "Mary Prado - Deep Inside Of Me"}, "spotify": {"spotify_id": null, "key": 0.0, "tempo": 1.1, "updated": null, "release_date": null, "album_image_url": null, "url": null}, "unmasked_id": 121547, "clean_title": "Deep Inside Me", "tracklist_title": "Deep Inside Of Me (Radio Edit)", "best_title": "Deep Inside Of Me"},
{"id": 67953479, "url": "http://www.youtube.com/watch?v=6NvOLyBXoyE", "title": "Mary Prado - Deep Inside Me (Official Music Video)", "channel": "Cloud 9 Music", "channel_id": "UC_411v5XTGmfSfWD9Cpk-Zw", "published": "2011-05-19T14:32:37", "error": null, "views": 3499.0, "tags": "Mary,Prado,Deep,Inside,Me,Mark,van,Dale,vs,DJ,Team,Music,Old,School,Happy,Hardcore,Gabber,Official,Video,HD,Cloud,widescreen,dance music,rave,clubbing,new,your,techno,trance,oldskool,mix", "updated": "2022-06-01T16:58:31", "topics": null, "discogs": {"country": "Netherlands", "year": 1997.0, "genre": "Electronic", "style": "Trance|Euro House|Italodance", "label": "High Voltage (9)", "uri": "https://www.discogs.com/release/1529220-Mary-Prado-Deep-Inside-Of-Me", "updated": "2022-06-01T15:24:21", "master_id": 187625.0, "thumb": "https://i.discogs.com/UFvXGF1K8OXexx6qI-s7PMWBWKSpSkQWUbVdetfoIGo/rs:fit/g:sm/q:40/h:150/w:150/czM6Ly9kaXNjb2dz/LWRhdGFiYXNlLWlt/YWdlcy9SLTE1Mjky/MjAtMTIyNzEwNDY0/My5qcGVn.jpeg", "cover_image": "https://i.discogs.com/0rwd7I6htGQHN4rLZf0VsgE0_Me3LNbRaL_5-MdZpMU/rs:fit/g:sm/q:90/h:439/w:600/czM6Ly9kaXNjb2dz/LWRhdGFiYXNlLWlt/YWdlcy9SLTE1Mjky/MjAtMTIyNzEwNDY0/My5qcGVn.jpeg", "title": "Deep Inside Of Me", "format": "CD", "artist": "Mary Prado", "data_quality": "Needs Vote", "copyright": null, "phonographic_copyright": null, "tracklist": "Deep Inside Of Me (Radio Edit)|Deep Inside Of Me (Extended)|Deep Inside Of Me (Techno Remix)|Deep Inside Of Me (Trance Remix)", "release_id": 1529220.0, "rating_count": 22.0, "rating_average": 4.73, "genre_regex": "<Electronic>", "style_regex": "<Trance>|<Euro House>|<Italodance>", "country_regex": "<Netherlands>", "artist_title": "Mary Prado - Deep Inside Of Me"}, "spotify": {"spotify_id": null, "key": null, "tempo": null, "updated": null, "release_date": null, "album_image_url": null, "url": null}, "unmasked_id": 121547, "clean_title": "Deep Inside Me", "tracklist_title": "Deep Inside Of Me (Radio Edit)", "best_title": "Deep Inside Of Me"}
]
""">

type SampletteDirectReq = JsonProvider<"""
{"id":67953479,"exclude":[],"kind":"direct","count":1}
""">

type LambdaPayload = {url:string; spleet:Nullable<bool>}

let keyMapping = [|"C";"C#";"D";"D#";"E";"F";"F#";"G";"G#";"A";"A#";"B"|] |> Array.map (fun n -> (n+"maj"))

let getSampletteData (id:string) =
    let uri = "https://samplette.io/get_sample"
    let payload = new SampletteDirectReq.Root(int(id),[||],"direct",1)
                    |> (fun n -> n.JsonValue.ToString())
    let headers = [
        "origin","https://samplette.io";
        "referer","https://samplette.io/"+id;
        "accept","*/*";
    ]
    let response = Http.RequestString(uri, httpMethod="POST", headers=headers, body=TextRequest (payload))
    printfn "%A" response
    response
        |> SampletteResponse.Parse
        |> Array.head
        |> (fun n -> (n.Url, n.Spotify.Key |> Option.map (fun n -> n |> int |> Array.get keyMapping), n.Spotify.Tempo))

let pullYoutubeVideo (destinationDir:string) (url:string) (key:string) (bpm:string) =
    let destination = Path.Combine(destinationDir,"%(title)s_%(id)s_"+key+"_"+bpm+"bpm.%(ext)s")
    let arguments = new StringBuilder()
    arguments.Append (" -v") |> ignore
    arguments.Append (" -x " + url) |> ignore
    arguments.Append (" -o \""+destination+"\"") |> ignore
    arguments.Append (" --cache-dir " + Path.GetTempPath()) |> ignore
    let startInfo = new ProcessStartInfo("youtube-dl", arguments.ToString())
    startInfo.RedirectStandardOutput <- true
    startInfo.UseShellExecute <- false
    let dlProcess = new Process()
    dlProcess.StartInfo <- startInfo
    dlProcess.Start() |> ignore
    dlProcess.WaitForExit(1000*300) |> ignore
    let processOut = dlProcess.StandardOutput.ReadToEnd()
    printfn "%A" processOut

    let alreadyExists = processOut.Contains("exists, skipping")

    match alreadyExists with
    | true ->
            processOut
            |> (fun n -> Regex.Match(n, "file (.*) exists,"))
            |> (fun n -> n.Groups.Item 1)
            |> (fun n -> n.Value)
    | false ->
            processOut
            |> (fun n -> Regex.Match(n, ".ffmpeg. Destination: (.*)"))
            |> (fun n -> n.Groups.Item 1)
            |> (fun n -> n.Value)

let spleetAudio (spleetDir:string) (audioLocation:string)  =
    let arguments = new StringBuilder()
    arguments.Append (" separate") |> ignore
    arguments.Append (" \"" + audioLocation+"\"") |> ignore
    arguments.Append (" -p spleeter:4stems") |> ignore
    arguments.Append (" -o \""+spleetDir+"\"") |> ignore
    arguments.Append (" --verbose") |> ignore
    let startInfo = new ProcessStartInfo("spleeter", arguments.ToString())
    startInfo.UseShellExecute <- false
    startInfo.RedirectStandardOutput <- true
    let processToStart = Process.Start(startInfo)
    processToStart.WaitForExit() |> ignore
    let processOut = processToStart.StandardOutput.ReadToEnd()
    printfn "%A" processOut

let ffprobeHealthCheck() =
    let arguments = new StringBuilder()
    arguments.Append (" -version") |> ignore
    let startInfo = new ProcessStartInfo("ffprobe", arguments.ToString())
    startInfo.UseShellExecute <- false
    startInfo.RedirectStandardOutput <- true
    let processToStart = Process.Start(startInfo)
    processToStart.WaitForExit() |> ignore
    let processOut = processToStart.StandardOutput.ReadToEnd()
    printfn "%A" processOut

let uploadFileToS3 (client: Amazon.S3.AmazonS3Client) (bucketName: string) (prefix:string) (file: FileInfo)  = 
    let uploadRequest = new UploadPartRequest()
    uploadRequest.BucketName <- bucketName
    uploadRequest.Key <- prefix + "/" + file.Name
    uploadRequest.FilePath <- file.FullName
    printfn "Uploading %s to prefix %s" file.FullName prefix
    client.UploadPartAsync(uploadRequest) 
    |> Async.AwaitTask 
    |> Async.Ignore

let checkIfS3PrefixExists (client: Amazon.S3.AmazonS3Client) (bucketName: string) (prefix:string) = 
    let request = new ListObjectsV2Request()
    request.BucketName <- bucketName
    request.Prefix <- prefix
    async {
        let! response = client.ListObjectsV2Async(request) |> Async.AwaitTask
        printfn "Found %d objects in prefix %s" response.S3Objects.Count prefix
        return response.S3Objects.Count > 0
    }
    
let pullAndSpleet (payload: LambdaPayload) (lambdaContext: ILambdaContext) =
    let bucketName = System.Environment.GetEnvironmentVariable("S3_BUCKET") |> Option.ofObj

    let (fileUploader, fileExistChecker) = match bucketName with
                                                    | Some bucket -> 
                                                        let client = new Amazon.S3.AmazonS3Client()
                                                        (uploadFileToS3 client bucket, checkIfS3PrefixExists client bucket)
                                                    | None -> 
                                                        ((fun s f -> async{ignore()}), (fun s -> async{return false} ))
    
    let audioDir = Path.Combine(Path.GetTempPath(), "Audio")
    Directory.CreateDirectory(audioDir) |> ignore
    let spleetDir = Path.Combine(Path.GetTempPath(), "Spleet")
    Directory.CreateDirectory(spleetDir) |> ignore
    [audioDir; spleetDir]
        |> List.map (fun n -> new DirectoryInfo(n))
        |> List.map (fun n -> n.EnumerateFiles("*", SearchOption.AllDirectories) |> seq)
        |> Seq.concat
        |> Seq.iter (fun n -> n.Delete())
    let url = payload.url
    
    ffprobeHealthCheck() |> ignore

    let (youtubeUrl, key, tempo) = match url with
                                    | n when n.ToLower().Contains("youtube") -> (url,Option.None,Option.None)
                                    | _ ->
                                            getSampletteData url

    let youtubeId = youtubeUrl.Split("v=") |> Array.last
    let prefixExists = fileExistChecker youtubeId |> Async.RunSynchronously
    if not(prefixExists) then
        printfn "Extracting youtube audio from %A" youtubeUrl
        let pulledAudio = pullYoutubeVideo audioDir youtubeUrl (key|>Option.defaultValue "null") (tempo |> Option.map string |> Option.defaultValue "null")
        let pulledAudioInfo = FileInfo(pulledAudio)
        printfn "Extracted youtube audio to %A" pulledAudio
        let shouldSpleet = Option.ofNullable payload.spleet |> Option.defaultValue true
        match (pulledAudioInfo.Exists, shouldSpleet) with
        | (_,false) -> ignore()
        | (true,_) ->
                spleetAudio spleetDir pulledAudio
        | (false,_) ->
                let audioLocation = Directory.EnumerateFiles(audioDir) |> Seq.filter(fun n -> n.Contains(youtubeId)) |> Seq.head
                spleetAudio spleetDir audioLocation
        

        let allAudioFiles = [audioDir; spleetDir]
                                                |> List.map (fun n -> new DirectoryInfo(n))
                                                |> List.map (fun n -> n.EnumerateFiles("*", SearchOption.AllDirectories) |> seq)
                                                |> Seq.concat
        printfn "Extracted audio files: %A" allAudioFiles
        let renameIfSpleet (fileName:string) =
            match fileName with
            | name when name = pulledAudioInfo.Name -> fileName
            | _ -> pulledAudioInfo.Name.Replace(pulledAudioInfo.Extension,"")+"_"+fileName

        let outputDir = Path.Combine(Path.GetTempPath(), "Output")
        Directory.CreateDirectory(outputDir) |> ignore                                        
        allAudioFiles
        |> Seq.iter (fun file -> file.CopyTo(Path.Combine(outputDir, (renameIfSpleet file.Name)), true) |> ignore)
        let zipPath = Path.Combine(Path.GetTempPath(), youtubeId+".zip")
        System.IO.Compression.ZipFile.CreateFromDirectory(outputDir, zipPath)
        fileUploader youtubeId (new FileInfo(zipPath))
        |> Async.RunSynchronously
        |> ignore
    printfn "Pull and spleet complete"

[<EntryPoint>]
let main argv = 
    let audioDir = Array.get argv 0
    let spleetDir = Array.get argv 1
    let url = Array.get argv 2
    let (youtubeUrl, key, tempo) = match url with
                                    | n when n.ToLower().Contains("youtube") -> (url,Option.None,Option.None)
                                    | _ ->
                                            getSampletteData url

    let youtubeId = youtubeUrl.Split("v=") |> Array.last
    printfn "Extracting youtube audio from %A" youtubeUrl
    let pulledAudio = pullYoutubeVideo audioDir youtubeUrl (key|>Option.defaultValue "null") (tempo |> Option.map string |> Option.defaultValue "null")
    printfn "Extracted youtube audio to %A" pulledAudio
    match (new FileInfo(pulledAudio)).Exists with
    | true ->
            spleetAudio spleetDir pulledAudio
    | false ->
            let audioLocation = Directory.EnumerateFiles(audioDir) |> Seq.filter(fun n -> n.Contains(youtubeId)) |> Seq.head
            spleetAudio spleetDir audioLocation
    printfn "Pull and spleet complete"
    0
