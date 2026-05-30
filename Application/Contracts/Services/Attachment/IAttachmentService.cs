using Utilities.Enums.AttachmentEnum;
using Application.Services.Attachment;
using Infrastructure.Services.Attachement.Input;
using Microsoft.AspNetCore.Http;
using Utilities.Enums.SystemConfigurationKeyEnum;

namespace Infrastructure.Services.Attachment;

public interface IAttachmentService
{
    void UploadAudioBase64(UploadAttachmentInput _input);
    void UploadBase64(UploadAttachmentInput _input);
    string SaveBase64(SaveAttachmentInput _input);
    void DeleteFromTemp(string fileName);
    void DeleteFileFromFolder(AttachmentPathEnum mainFolder, string folderName, string fileName);
    byte[] Download(AttachmentPathEnum type, string relativePath);
    byte[] Download(string serverPath);
    string DownloadBase64(string serverPath);
    Task<string> DownloadBase64Async(string serverPath);
    string GetFullPath(string serverPath);
    string DownloadTameBase64(string serverPath);
    Action GetMoveFromTempAction(AttachmentPathEnum type, string oldRelativePath, string newRelativePath);
    Action GetMoveFromTempAction(string tempRelativePath, string newFullPath);
    Action GetCopyAction(AttachmentPathEnum oldType, string oldRelativePath, AttachmentPathEnum newType, string newRelativePath);
    Action GetCopyAction(string oldRelativePath, AttachmentPathEnum newType, string newRelativePath);
    Action GetCopyToNewRequestAction(AttachmentPathEnum type, string oldRelativePath, string newRelativePath);
    string GetFullPath(AttachmentPathEnum type, string relativePath);
    string GetUrlPath(string url);
    string GetTempPath(string relativePath);
    bool CheckIfFileExist(AttachmentPathEnum type, string relativePath);
    string UploadBinaryFile(IFormFile file, Func<SystemConfigurationKeyEnum, string> FindConfigurationValue);
    Task<DownloadFileResult> DownloadLargeFile(string relativePath);
}
