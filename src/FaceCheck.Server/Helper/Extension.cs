using FaceCheck.Server.Configs;
using FaceCheck.Server.Model;
using FaceCheck.Server.Util;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Net.Mime;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;

namespace FaceCheck.Server.Helper
{
    public static class Extension
    {
        /// <summary>
        /// 判断是否为人脸图片 (png jpg bmp)
        /// </summary>
        /// <param name="imageBuffer"> </param>
        /// <returns> </returns>
        public static bool IsFacePhoto(this byte[] imageBuffer)
        {
            bool result = false;
            if (imageBuffer != null
                && imageBuffer.Length > 8)
            {
                var header = imageBuffer.Take(8);

                result = PicHeader.PngHeader.SequenceEqual(header)
                    || PicHeader.BmpHeader.SequenceEqual(header.Take(PicHeader.BmpHeader.Length))
                    || PicHeader.JpgHeader.SequenceEqual(header.Take(PicHeader.JpgHeader.Length))
                    || PicHeader.GifHeader1.SequenceEqual(header.Take(PicHeader.GifHeader1.Length))
                    || PicHeader.GifHeader2.SequenceEqual(header.Take(PicHeader.GifHeader2.Length));
            }
            return result;
        }

        /// <summary>
        /// 获取图片文件类型 只支持 png gif bmp tiff icon jpg
        /// </summary>
        /// <param name="imageBuffer"> </param>
        /// <returns> </returns>
        public static string GetPicExtention(this byte[] imageBuffer)
        {
            string result = string.Empty;
            if (imageBuffer != null
                && imageBuffer.Length > 8)
            {
                var header = imageBuffer.Take(8);
                if (PicHeader.TgaHeader1.SequenceEqual(header.Take(PicHeader.TgaHeader1.Length))
                || PicHeader.TgaHeader2.SequenceEqual(header.Take(PicHeader.TgaHeader2.Length)))
                {
                    // result = ImageFormat.TGA;
                }
                else if (PicHeader.CurHeader.SequenceEqual(header))
                {
                    // result = ImageFormat.CUR;
                }
                else if (PicHeader.PngHeader.SequenceEqual(header))
                {
                    result = ".png";
                }
                else if (PicHeader.GifHeader1.SequenceEqual(header.Take(PicHeader.GifHeader1.Length))
                    || PicHeader.GifHeader2.SequenceEqual(header.Take(PicHeader.GifHeader2.Length)))
                {
                    result = ".gif";
                }
                else if (PicHeader.BmpHeader.SequenceEqual(header.Take(PicHeader.BmpHeader.Length)))
                {
                    result = ".bmp";
                }
                else if (PicHeader.TiffHeader1.SequenceEqual(header.Take(PicHeader.TiffHeader1.Length))
                    || PicHeader.TiffHeader2.SequenceEqual(header.Take(PicHeader.TiffHeader2.Length)))
                {
                    result = ".tiff";
                }
                else if (PicHeader.IconHeader.SequenceEqual(header))
                {
                    result = ".icon";
                }
                else if (PicHeader.JpgHeader.SequenceEqual(header.Take(PicHeader.JpgHeader.Length)))
                {
                    result = ".jpg";
                }
            }
            return result;
        }

        /// <summary>
        /// </summary>
        /// <returns> </returns>
        public static IServiceCollection ConfigureServices(this IServiceCollection services, SystemConfig systemConfig)
        {
            services.AddHealthChecks();
            services.AddJsonOptions(systemConfig.PrettyPrintingJson);
            services.SetPostConfigure();
            services.AddMemoryCache();

            services.Configure<FormOptions>(options =>
            {
                options.KeyLengthLimit = int.MaxValue;
                options.ValueLengthLimit = int.MaxValue;
                options.MultipartHeadersLengthLimit = int.MaxValue;
                options.MultipartBodyLengthLimit = long.MaxValue;
                options.MemoryBufferThreshold = int.MaxValue;
            });
            services.Configure<KestrelServerOptions>(options =>
            {
                options.Limits.MaxRequestBodySize = int.MaxValue;
                options.Limits.MaxRequestBufferSize = int.MaxValue;
            });

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            services.AddEndpointsApiExplorer();
            return services;
        }

        /// <summary>
        /// </summary>
        /// <returns> </returns>
        public static IServiceCollection AddJsonOptions(this IServiceCollection services, bool prettyPrintingJson)
        {
            services.ConfigureHttpJsonOptions(options =>
            {
                options.SerializerOptions.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All);
                // 首字母小写驼峰命名
                options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                // 允许注释
                options.SerializerOptions.ReadCommentHandling = JsonCommentHandling.Skip;
                options.SerializerOptions.NumberHandling = JsonNumberHandling.AllowReadingFromString;
                // 允许尾随逗号
                options.SerializerOptions.AllowTrailingCommas = true;
                options.SerializerOptions.WriteIndented = prettyPrintingJson;
                options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.Never;
            });
            return services;
        }

        /// <summary>
        /// 错误参数信息拦截
        /// </summary>
        /// <returns> </returns>
        public static IServiceCollection SetPostConfigure(this IServiceCollection services)
        {
            services.PostConfigure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = (context) =>
                {
                    var factory = context.HttpContext.RequestServices.GetRequiredService<ProblemDetailsFactory>();
                    var details = factory.CreateValidationProblemDetails(context.HttpContext, context.ModelState);
                    if (details != null)
                    {
                        if (details.Status.HasValue)
                        {
                            context.HttpContext.Response.StatusCode = details.Status.Value;
                        }
                        context.HttpContext.Response.ContentType = MediaTypeNames.Application.Json;

                        var result = new HttpResult
                        {
                            Success = false,
                            Code = context.HttpContext.Response.StatusCode,
                        };
                        if (!string.IsNullOrWhiteSpace(details.Detail))
                        {
                            result.Message = details.Detail;
                        }
                        else
                        {
                            if (details.Errors.Any())
                            {
                                foreach (var error in details.Errors)
                                {
                                    result.Message += $"{error.Key}:{string.Join(" ", error.Value)} ";
                                }
                            }
                        }
                        return new JsonResult(result);
                    }
                    return null;
                };
            });
            return services;
        }

        /// <summary>
        /// 错误参数信息拦截
        /// </summary>
        /// <returns> </returns>
        public static WebApplication AppInit(this WebApplication app, ConfigurationManager configuration, SystemConfig systemConfig)
        {
            #region 解决Ubuntu Nginx 代理不能获取IP问题

            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.All
            });
            app.UseStatusCodePagesWithReExecute("/error/{0}.html");

            #endregion 解决Ubuntu Nginx 代理不能获取IP问题

            app.UseHealthChecks("/healthcheck");
            app.UseExceptionHandlingMiddleware();
            app.UseRouting();

            app.Use(async (context, next) =>
            {
                if (!context.WebSockets.IsWebSocketRequest
                   && context.Request.Headers.Accept == "text/plain")
                {
                    context.Request.Headers.Accept = $"{System.Net.Mime.MediaTypeNames.Application.Json},*/*";
                }
                await next();
            });

            if (systemConfig.IsDev)
            {
                app.UseDeveloperExceptionPage();
            }

            #region 静态文件服务功能

            // 提供默认的文件

            //var options = new DefaultFilesOptions();
            //options.DefaultFileNames.Clear();
            //options.DefaultFileNames.Add("index.html");
            //app.UseDefaultFiles(options);

            app.UseDefaultFiles();
            if (systemConfig.UseDirectoryBrowser)
            {
                app.UseDirectoryBrowser();
            }

            // 提供静态文件
            var staticfile = new StaticFileOptions
            {
                ServeUnknownFileTypes = true,
                DefaultContentType = "application/octet-stream" //设置默认  MIME
            };

            var provider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();

            foreach (var item in provider.Mappings)
            {
                if (item.Value.StartsWith("text/")
                    || item.Value.StartsWith(MediaTypeNames.Application.Json))
                {
                    provider.Mappings[item.Key] = item.Value + ";charset=utf-8";
                }
            }

            var mimeSection = configuration.GetChildren().FirstOrDefault(record => record.Key == "MIME");
            if (mimeSection != null)
            {
                foreach (var mime in mimeSection.GetChildren())
                {
                    if (provider.Mappings.ContainsKey(mime.Key))
                    {
                        provider.Mappings[mime.Key] = mime.Value;
                    }
                    else
                    {
                        provider.Mappings.Add(mime.Key, mime.Value);
                    }
                }
            }

            staticfile.ContentTypeProvider = provider;
            app.UseStaticFiles(staticfile);

            #endregion 静态文件服务功能

            return app;
        }
    }

    internal static class PicHeader
    {
        /// <summary>
        /// </summary>
        public static byte[] TgaHeader1 { get; } = [0x00, 0x00, 0x02, 0x00, 0x00];

        /// <summary>
        /// </summary>
        public static byte[] TgaHeader2 { get; } = [0x00, 0x00, 0x10, 0x00, 0x00];

        /// <summary>
        /// </summary>
        public static byte[] CurHeader { get; } = [0x00, 0x00, 0x02, 0x00, 0x01, 0x00, 0x20, 0x20];

        /// <summary>
        /// </summary>
        public static byte[] PngHeader { get; } = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];

        /// <summary>
        /// </summary>
        public static byte[] GifHeader1 { get; } = [0x47, 0x49, 0x46, 0x38, 0x39, 0x61];

        /// <summary>
        /// </summary>
        public static byte[] GifHeader2 { get; } = [0x47, 0x49, 0x46, 0x38, 0x37, 0x61];

        /// <summary>
        /// </summary>
        public static byte[] BmpHeader { get; } = [0x42, 0x4D];

        /// <summary>
        /// </summary>
        public static byte[] TiffHeader1 { get; } = [0x4D, 0x4D];

        /// <summary>
        /// </summary>
        public static byte[] TiffHeader2 { get; } = [0x49, 0x49];

        /// <summary>
        /// </summary>
        public static byte[] IconHeader { get; } = [0x00, 0x00, 0x01, 0x00, 0x01, 0x00, 0x20, 0x20];

        /// <summary>
        /// </summary>
        public static byte[] JpgHeader { get; } = [0xff, 0xd8];
    }
}