using UnityEngine;
using System.Collections;
using Atoms;
using RSG;
using System;
using JsonFx.Json;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using System.IO;
using Karma.Metadata;
using Arista.Models;
using Arista.Services;

namespace Karma
{
    [Service]
    public class Fetch
    {
        public Env env { get; private set; }
        public IInternetInformationProvider iip { get; private set; }

        public Fetch(Env env, IInternetInformationProvider iip)
        {
            this.env = env;
            this.iip = iip;
        }

        public IPromise<A> LocalResource<A>(string path, IEnumLoader m, Action<float> onProgress = null) where A : UnityEngine.Object
        {
            var promise = new Promise<A>();
            var request = Resources.LoadAsync(path);
            var progress = 0f;

            Atom.While(() => !request.isDone, () => 
            {
                if (onProgress != null && progress != request.progress)
                {
                    onProgress(request.progress);
                    progress = request.progress;
                }
            })
            .Then(() =>
            {
                var go = GameObject.Instantiate(request.asset) as GameObject;
                promise.Resolve(go.GetComponent<A>());
            })
            .StartLoad(m);

            return promise;
        }

        public IPromise<WWW> LoadWWW(WWW request, IEnumLoader m, Action<float> onProgress = null, bool dispose = true)
        {
            var promise = new Promise<WWW>();
            Debug.Log("DOWNLOADING " + request.url);

            Atom.WaitWhile(() =>
            {
                if (onProgress != null) {
                    onProgress(request.progress);
                }

                return !request.isDone;
            })
            .Then(() =>
            {
                if (onProgress != null)
                {
                    onProgress(1f);
                }

                Debug.Log("AFTER LOAD");
                if (request.error == null)
                {
                    promise.Resolve(request);

                    if (dispose)
                    {
                        request.Dispose();
                        request = null;
                    }
                }
                else
                {
                    Debug.Log("Download Error: " + request.error);
                    promise.Reject(new Exception(request.error));
                }
            })
            .CatchError<Exception>(promise.Reject)
            .StartLoad(m);

            return promise;
        }

        public IPromise<string> String(string url, IEnumLoader m, byte[] postData = null, Dictionary<string, string> headers = null, Action<float> onProgress = null)
        {
            WWW request = null;

            if (postData == null)
            {
                request = new WWW(url);
            }
            else
            {
                if (headers == null)
                {
                    headers = new Dictionary<string, string>();
                }

                request = new WWW(url, postData, headers);
            }


            return LoadWWW(request, m, onProgress: onProgress)
                .Then((response) =>
                {
                    return response.text;
                });
        }
        
        public IPromise<string> GetStringCached(string url, IEnumLoader m, Action<float> onProgress = null)
        {
            return GetCached(url, m, onProgress, dispose: false)
                .Then(www =>
                {
                    var s = www.text;
                    www.Dispose();
                    return www.text;
                });
        }


        public IPromise<A> RequestDecoded<A>(string url, IEnumLoader m, byte[] postData = null, Dictionary<string, string> headers = null, Action<float> onProgress = null)
        {
            return String(url, m, postData: postData, headers: headers, onProgress: onProgress)
                .Then((string json) => {
                    Debug.Log(json);
                    return Json.Deserialize<A>(json);
                });
        }

        public IPromise<A> PostRequestDecoded<A>(string url, object dataObject, IEnumLoader m, Dictionary<string, string> headers = null, Action<float> onProgress = null)
        {
            var stringRep = Json.Serialize(dataObject);
            var postData = Encoding.ASCII.GetBytes(stringRep);

            return RequestDecoded<A>(url, m, postData: postData, headers: headers, onProgress: onProgress);
        }

        public IPromise<A> JsonPostRequestDecoded<A>(string url, object dataObject, IEnumLoader m, Dictionary<string, string> headers = null, Action<float> onProgress = null)
        {
            if (headers == null)
            {
                headers = new Dictionary<string, string>();
            }

            headers["Content-Type"] = "application/json";

            return PostRequestDecoded<A>(url, dataObject, m, headers: headers, onProgress: onProgress);
        }

        public IPromise<GameObject> LoadAssetBundle(WWW response, IEnumLoader m, Action<float> onProgress = null)
        {
            var future = new Promise<GameObject>();

            Debug.Log(response.error);
            var _asset = response.assetBundle;

            foreach (var name in _asset.GetAllAssetNames())
            {
                //Debug.Log(name);
            }
            //future.Complete((GameObject)UnityEngine.Object.Instantiate(_asset.LoadAsset<GameObject>("unidad")));

            var assetBundleRequest = _asset.LoadAssetAsync<GameObject>("aristaGameObject");
            var progress = 0f;

            Debug.Log("AFTER NAMES");
            Atom.WaitWhile(() =>
            {
                if (onProgress != null && progress != assetBundleRequest.progress)
                {
                    onProgress(assetBundleRequest.progress);
                    progress = assetBundleRequest.progress;
                }

                return !assetBundleRequest.isDone;
            })
            .Then(() =>
            {
                if (onProgress != null)
                {
                    onProgress(1f);
                }

                Debug.Log("ASSET BUNDLE");
                Debug.Log(assetBundleRequest);
                if (assetBundleRequest.asset != null)
                {
                    Debug.Log("Non NULL");
                    future.Resolve((GameObject)UnityEngine.Object.Instantiate(assetBundleRequest.asset));
                }
                else
                {
                    future.Reject(new NullReferenceException());
                }

                if (_asset != null)
                    _asset.Unload(false);
            })
            .CatchError<Exception>(future.Reject)
            .StartLoad(m);

            return future;
        }



        public IPromise<GameObject> RequestGameObject(string url, int version, IEnumLoader m, Action<float> onProgress = null)
        {

            var request = WWW.LoadFromCacheOrDownload(url, version);

            Debug.Log("Request GO");

            return LoadWWW(request, m, onProgress: onProgress)
                .Then(response => LoadAssetBundle(response, m, onProgress: onProgress));
        }

        public IPromise<Texture2D> Texture(string url, Texture2D texture, IEnumLoader m, Action<float> onProgress = null)
        {
            return LoadWWW(new WWW(url), m, onProgress)
                .Then(www =>
                {
                    www.LoadImageIntoTexture(texture);
                    return texture;
                });
        }

        public IPromise<Texture2D> TextureCached(string url, Texture2D texture, IEnumLoader m, Action<float> onProgress = null, float maxHours = 1)
        {
            return GetCached(url, m, onProgress, maxHours: maxHours, dispose: false)
                .Then(www =>
                {
                    www.LoadImageIntoTexture(texture);
                    www.Dispose();
                    return texture;
                });
        }

        public IPromise<WWW> GetCached(string url, IEnumLoader m, Action<float> onProgress = null, float maxHours = 1, bool hasInternet = true, bool dispose = true)
        {
            var hash = HashSHA1(url);
            var dirPath = Application.persistentDataPath + "/cache/";
            string path = dirPath + hash;

            //Create forlder if does not exist
            if (! Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }

            WWW www;
            var fileExists = File.Exists(path);
            var expired = false;

            if (fileExists)
            {
                //check how old
                DateTime written = File.GetLastWriteTimeUtc(path);
                DateTime now = DateTime.UtcNow;
                double totalHours = now.Subtract(written).TotalHours;
                if (totalHours > env.cache_hours)
                    expired = true;
            }

            var downloadFromCache = fileExists && (!expired || !hasInternet);

            if (downloadFromCache)
            {
                string localPath = "file://" + path;
                Debug.Log("TRYING FROM CACHE " + url + "  file " + localPath);
                www = new WWW(localPath);
            }
            else
            {
                www = new WWW(url);
            }

            
            return LoadWWW(www, m, onProgress, dispose: dispose)
                .Then(_www =>
                {
                    if (! downloadFromCache)
                    {
                        CacheRequest(_www, path);
                    }
                });
        }

        static void CacheRequest(WWW www, string filePath)
        {
            Debug.Log("SAVING DOWNLOAD  " + www.url + " to " + filePath);
            File.WriteAllBytes(filePath, www.bytes);
            Debug.Log("SAVING DONE  " + www.url + " to " + filePath);
        }

        public string HashSHA1(string value)
        {
            var sha1 = SHA1.Create();
            var inputBytes = Encoding.ASCII.GetBytes(value);
            var hash = sha1.ComputeHash(inputBytes);
            var sb = new StringBuilder();
            for (var i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }
            return sb.ToString();
        }
    }

    

    public static class Json
    {
        public static A Deserialize<A>(string s)
        {
            JsonReaderSettings readerSettings = new JsonReaderSettings();
            readerSettings.TypeHintName = "__type__";
            return new JsonReader(s, readerSettings).Deserialize<A>();
        }

        public static IEnumerable<A> DeserializeList<A>(string s)
        {
            JsonReaderSettings readerSettings = new JsonReaderSettings();
            readerSettings.TypeHintName = "__type__";
            JsonReader reader = new JsonReader(s, readerSettings);
            return new JsonReader(s, readerSettings).Deserialize<A[]>();
        }

        public static string Serialize<A>(A a)
        {
            JsonWriterSettings writerSettings = new JsonWriterSettings();
            writerSettings.TypeHintName = "__type__";
            StringBuilder json = new StringBuilder();
            JsonWriter writer = new JsonWriter(json, writerSettings);
            writer.Write(a);
            return json.ToString();
        }

        public static void StartLoad(this IEnumerable e, IEnumLoader loader)
        {
            loader.Load(e);
        }
    }

    

    public class FakeLoader : IEnumLoader
    {
        public IEnumerable e { get; private set; }

        public void Load(IEnumerable e)
        {
            this.e = e;
        }

        public void StartLoad() {
            e.Run();
        }
    }

    public interface IEnumLoader
    {
        void Load(IEnumerable e);
    }
}
