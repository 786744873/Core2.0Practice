using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using RazorPagesMovie.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RazorPagesMovie.Utilities
{
    public static class FileHelpers
    {
        /// <summary>
        /// 上传文件
        /// </summary>
        /// <param name="formFile"></param>
        /// <param name="modelState"></param>
        /// <returns></returns>
        public static async Task<String> ProcessFormFile(IFormFile formFile, ModelStateDictionary modelState)
        {
            var filedDisplayName = String.Empty;

            /*
             * Use reflection to obtain the display name for the model property associated with this IFormFile. 
             * If a display name isn't found, error messages simply won't show a display name.
             * 
             * 通过反射获取到与IFormFile文件关联的模型的属性的特性名称
             * 如果未找到该特性名称，则不会显示
            */
            MemberInfo property = typeof(FileUpload).GetProperty(formFile.Name.Substring(formFile.Name.IndexOf(".") + 1));
            if (property!=null)
            {
                var displayAttribute = property.GetCustomAttribute(typeof(DisplayAttribute)) as DisplayAttribute;
                if (displayAttribute!=null)
                {
                    filedDisplayName = $"{displayAttribute.Name}";
                }
            }

            /*
             * Use Path.GetFileName to obtain the file name, which will strip any path information passed as part of the FileName property.
             * HtmlEncode the result in case it must be returned in an error message.
             * 
             * 使用Path.GetFileName获得文件名称，该文件名将作为文件属性的一部分传递任何路径信息
             * HtmlEncode的结果必须在错误消息中返回
             */
            var fileName = WebUtility.HtmlEncode(Path.GetFileName(formFile.Name));
            if (formFile.ContentType.ToLower()!="text/plain")
            {
                modelState.AddModelError(formFile.Name, $"The {filedDisplayName} file ({fileName}) must be a text file.");
            }

            /*
             * Check the file length and don't bother attempting to read it if the file contains no content. 
             * This check doesn't catch files that only have a BOM as their content, so a content length check is made later after reading the file's content to catch a file that only contains a BOM.
             * 
             * 检查文件长度，如果文件不包含任何内容，则不需要读取文件
             * 此检查不会捕获仅具有BOM作为内容的文件，因此在读取文件内容后捕获内容长度检查，以捕获仅包含BOM的文件。
             */
            if (formFile.Length==0)
            {
                modelState.AddModelError(formFile.Name, $"The {filedDisplayName} file ({fileName}) is empty.");
            }
            else if(formFile.Length>1048576)
            {
                modelState.AddModelError(formFile.Name, $"The {filedDisplayName} file ({fileName}) exceeds 1MB.");
            }
            else
            {
                try
                {
                    //上传文件
                    var fileSavePath = Path.Combine(Directory.GetCurrentDirectory().Split("bin")[0], "UploadFile", DateTime.Now.ToString("yyyyMMddHHmmss") + "." + Path.GetExtension(formFile.Name));
                    using (var fileStream = new FileStream(fileSavePath, FileMode.Create))
                    {
                        await formFile.CopyToAsync(fileStream);
                    }


                    string fileContents;
                    using (var reader=new StreamReader(formFile.OpenReadStream(),new UTF8Encoding(encoderShouldEmitUTF8Identifier:false,throwOnInvalidBytes:true),detectEncodingFromByteOrderMarks:true))
                    {
                        fileContents = await reader.ReadToEndAsync();
                        if (!string.IsNullOrEmpty(fileContents))
                        {
                            return fileContents;
                        }
                        else
                        {
                            modelState.AddModelError(formFile.Name, $"The {filedDisplayName} file ({fileName}) is empty");
                        }
                    }

                    
                }
                catch (Exception ex)
                {
                    modelState.AddModelError(formFile.Name, $"The {filedDisplayName} file ({fileName}) upload failed. Please contact the Help Disk for support. Error:{ex.Message}");
                    // TODO Log
                }
            }

            return string.Empty;

        }
    }
}
