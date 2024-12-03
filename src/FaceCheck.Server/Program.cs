using FaceCheck.Server.Configs;
using FaceCheck.Server.Helper;
using FaceCheck.Server.Helper.Configs;
using FaceCheck.Server.Model;
using FaceCheck.Server.Util;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace FaceCheck.Server
{
    public class Program
    {
        private static SystemConfig SystemConfig { set; get; } = new SystemConfig();
        private static ArcSoftConfig ArcSoftConfig { set; get; } = new ArcSoftConfig();

        /// <summary>
        /// </summary>
        private static Microsoft.Extensions.Logging.ILogger Logger { set; get; }

        public static void Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var builder = WebApplication.CreateSlimBuilder(args);
            builder.Configuration.Bind(SystemConfig.Section, SystemConfig);
            builder.Configuration.Bind(ArcSoftConfig.Section, ArcSoftConfig);

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Win32.DisbleQuickEditMode();
                Win32.ConsoleCtrlDelegate newDelegate = new(HandlerRoutine);
                if (!Win32.SetConsoleCtrlHandler(newDelegate, true))
                {
                    var defaultColor = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("抱歉，API注入失败，按任意键退出！");
                    Console.ForegroundColor = defaultColor;
                    Console.ReadKey();
                    return;
                }

                if (SystemConfig.Hide)
                {
                    Win32.ShowWindow(Win32.GetConsoleWindow(), Win32.SW_HIDE);
                }
            }

            if (!string.IsNullOrWhiteSpace(SystemConfig.Title))
            {
                Console.Title = SystemConfig.Title;
            }

            builder.Services.AddSingleton<ArcFaceUtil>()
                            .AddSingleton<PhotoCheck>()
                            .ConfigureServices(SystemConfig)
                            .AddLogging(r =>
                            {
#if DEBUG
                                r.AddDebug();
#endif
                                r.ClearProviders()
                                 .SetMinimumLevel(LogLevel.Trace)
                                 .AddConsole();
                            })
                            .AddSerilog((services, lc) => lc
                                        .ReadFrom.Configuration(builder.Configuration)
                                        .ReadFrom.Services(services)
                                        .Enrich.FromLogContext()
                                        );

            var app = builder.Build();
            Logger = app.Logger;
            var informationalVersion = Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
            app.Logger.LogInformation("Version: {Version}", informationalVersion);

            SetThreadPool();

            app.UseSerilogRequestLogging();
            app.AppInit(builder.Configuration, SystemConfig);

            var arcFaceUtil = app.Services.GetService<ArcFaceUtil>();
            arcFaceUtil.Activation();
            app.MapGet("/healthcheck", () =>
                {
                    return "Healthy";
                });

            app.MapPost("/", (SetPicInfo comparePicInfo) =>
            {
                try
                {
                    var photoCheck = app.Services.GetService<PhotoCheck>();
                    photoCheck.ReadyUrlBuffer(ref comparePicInfo);

                    if (comparePicInfo != null
                        && comparePicInfo.Pic1 != null
                        && comparePicInfo.Pic1.Length > 0)
                    {
                        if (comparePicInfo.Pic2 == null
                        || comparePicInfo.Pic2.Length == 0)
                        {
                            var result = photoCheck.CheckFace(comparePicInfo.Pic1);
                            return Results.Ok(result);
                        }
                        else
                        {
                            var result = photoCheck.PicCompare(comparePicInfo.Pic1, comparePicInfo.Pic2);
                            return Results.Ok(result);
                        }
                    }
                    return Results.Ok(new HttpResult { Success = false, Message = "数据不正确" });
                }
                catch (Exception ex)
                {
                    return Results.Ok(new HttpResult
                    {
                        Success = false,
                        Message = ex.Message,
                    });
                }
            });

            app.MapPost("/", async (HttpRequest request) =>
            {
                var form = await request.ReadFormAsync();
                if (form.Files != null
                && form.Files.Count > 0)
                {
                    await using var stream = new BufferedStream(form.Files.ToList().FirstOrDefault().OpenReadStream());
                    var comparePicInfo = new SetPicInfo
                    {
                        Pic1 = new byte[stream.Length]
                    };
                    await stream.ReadAsync(comparePicInfo.Pic1, 0, (int)stream.Length);

                    var photoCheck = app.Services.GetService<PhotoCheck>();
                    try
                    {
                        photoCheck.ReadyUrlBuffer(ref comparePicInfo);

                        if (comparePicInfo != null
                            && comparePicInfo.Pic1 != null
                            && comparePicInfo.Pic1.Length > 0)
                        {
                            var result = photoCheck.CheckFace(comparePicInfo.Pic1);
                            return Results.Ok(result);
                        }
                        return Results.Ok(new HttpResult { Success = false, Message = "数据不正确" });
                    }
                    catch (Exception ex)
                    {
                        return Results.Ok(new HttpResult
                        {
                            Success = false,
                            Message = ex.Message,
                        });
                    }
                }
                else
                {
                    return Results.Ok(new HttpResult { Success = false, Message = "文件接收失败" });
                }
            }).Accepts<HttpRequest>("multipart/form-data");

            app.MapPost("/CheckPic2", (SetPicInfo comparePicInfo) =>
            {
                try
                {
                    var photoCheck = app.Services.GetService<PhotoCheck>();
                    photoCheck.ReadyUrlBuffer(ref comparePicInfo);

                    if (comparePicInfo != null
                        && comparePicInfo.Pic1 != null
                        && comparePicInfo.Pic1.Length > 0)
                    {
                        if (comparePicInfo.Pic2 == null
                        || comparePicInfo.Pic2.Length == 0)
                        {
                            var result = photoCheck.CheckFace2(comparePicInfo.Pic1);
                            return Results.Ok(result);
                        }
                        else
                        {
                            var result = photoCheck.PicCompare(comparePicInfo.Pic1, comparePicInfo.Pic2);
                            return Results.Ok(result);
                        }
                    }
                    return Results.Ok(new HttpResult { Success = false, Message = "数据不正确" });
                }
                catch (Exception ex)
                {
                    return Results.Ok(new HttpResult
                    {
                        Success = false,
                        Message = ex.Message,
                    });
                }
            });

            app.MapPost("/CheckPic2", async (HttpRequest request) =>
            {
                var form = await request.ReadFormAsync();
                if (form.Files != null
                && form.Files.Count > 0)
                {
                    await using var stream = new BufferedStream(form.Files.ToList().FirstOrDefault().OpenReadStream());
                    var comparePicInfo = new SetPicInfo
                    {
                        Pic1 = new byte[stream.Length]
                    };
                    await stream.ReadAsync(comparePicInfo.Pic1, 0, (int)stream.Length);

                    try
                    {
                        var photoCheck = app.Services.GetService<PhotoCheck>();
                        photoCheck.ReadyUrlBuffer(ref comparePicInfo);

                        if (comparePicInfo != null
                            && comparePicInfo.Pic1 != null
                            && comparePicInfo.Pic1.Length > 0)
                        {
                            var result = photoCheck.CheckFace2(comparePicInfo.Pic1);
                            return Results.Ok(result);
                        }
                        return Results.Ok(new HttpResult { Success = false, Message = "数据不正确" });
                    }
                    catch (Exception ex)
                    {
                        return Results.Ok(new HttpResult
                        {
                            Success = false,
                            Message = ex.Message,
                        });
                    }
                }
                else
                {
                    return Results.Ok(new HttpResult { Success = false, Message = "文件接收失败" });
                }
            }).Accepts<HttpRequest>("multipart/form-data");

            app.MapPost("/CheckIdPic", (SetPicInfo comparePicInfo) =>
            {
                try
                {
                    var photoCheck = app.Services.GetService<PhotoCheck>();
                    photoCheck.ReadyUrlBuffer(ref comparePicInfo);

                    if (comparePicInfo != null
                        && comparePicInfo.Pic1 != null
                        && comparePicInfo.Pic1.Length > 0)
                    {
                        if (comparePicInfo.Pic2 == null
                        || comparePicInfo.Pic2.Length == 0)
                        {
                            var result = photoCheck.CheckIdFace(comparePicInfo.Pic1);
                            return Results.Ok(result);
                        }
                        else
                        {
                            var result = photoCheck.PicCompare(comparePicInfo.Pic1, comparePicInfo.Pic2);
                            return Results.Ok(result);
                        }
                    }
                    return Results.Ok(new HttpResult { Success = false, Message = "数据不正确" });
                }
                catch (Exception ex)
                {
                    return Results.Ok(new HttpResult
                    {
                        Success = false,
                        Message = ex.Message,
                    });
                }
            });

            app.MapPost("/CheckIdPic", async (HttpRequest request) =>
            {
                var form = await request.ReadFormAsync();
                if (form.Files != null
                && form.Files.Count > 0)
                {
                    await using var stream = new BufferedStream(form.Files.ToList().FirstOrDefault().OpenReadStream());
                    var comparePicInfo = new SetPicInfo
                    {
                        Pic1 = new byte[stream.Length]
                    };
                    await stream.ReadAsync(comparePicInfo.Pic1, 0, (int)stream.Length);

                    try
                    {
                        var photoCheck = app.Services.GetService<PhotoCheck>();
                        photoCheck.ReadyUrlBuffer(ref comparePicInfo);

                        if (comparePicInfo != null
                            && comparePicInfo.Pic1 != null
                            && comparePicInfo.Pic1.Length > 0)
                        {
                            var result = photoCheck.CheckIdFace(comparePicInfo.Pic1);
                            return Results.Ok(result);
                        }
                        return Results.Ok(new HttpResult { Success = false, Message = "数据不正确" });
                    }
                    catch (Exception ex)
                    {
                        return Results.Ok(new HttpResult
                        {
                            Success = false,
                            Message = ex.Message,
                        });
                    }
                }
                else
                {
                    return Results.Ok(new HttpResult { Success = false, Message = "文件接收失败" });
                }
            }).Accepts<HttpRequest>("multipart/form-data");
            app.Run();
        }

        private static void SetThreadPool()
        {
            ThreadPool.GetMinThreads(out int workerThreadsMin, out int completionPortThreadsMin);
            Logger.LogDebug("获取当前 workerThreadsMin:{workerThreadsMin} completionPortThreadsMin:{completionPortThreadsMin}", workerThreadsMin, completionPortThreadsMin);
            ThreadPool.GetMaxThreads(out int workerThreadsMax, out int completionPortThreadsMax);
            Logger.LogDebug("获取当前 workerThreadsMax:{workerThreadsMax} completionPortThreadsMax:{completionPortThreadsMax}", workerThreadsMax, completionPortThreadsMax);
            ThreadPool.SetMinThreads(workerThreadsMax, completionPortThreadsMax);
            ThreadPool.GetMinThreads(out workerThreadsMin, out completionPortThreadsMin);
            Logger.LogDebug("获取设置后 workerThreadsMin:{workerThreadsMin} completionPortThreadsMin:{completionPortThreadsMin}", workerThreadsMin, completionPortThreadsMin);
        }

        private static bool HandlerRoutine(int dwCtrlType)
        {
            Console.WriteLine($"HandlerRoutine: {dwCtrlType}");
            return true;
        }
    }
}