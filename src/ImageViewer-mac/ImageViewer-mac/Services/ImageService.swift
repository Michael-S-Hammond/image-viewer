//
//  ImageService.swift
//  ImageViewer-mac
//
//  Created by Michael Hammond on 7/16/25.
//

import Foundation

class ImageService: ObservableObject {
    private static let supportedExtensions = [".png", ".jpg", ".jpeg", ".gif", ".bmp", ".tiff", ".tif", ".mp4", ".heic", ".heif", ".webp"]
    
    func loadImagesFromFolder(_ folderURL: URL) throws -> [ImageInfo] {
        guard folderURL.hasDirectoryPath else {
            throw ImageServiceError.invalidFolder
        }
        
        let fileManager = FileManager.default
        let resourceKeys: [URLResourceKey] = [.isRegularFileKey, .fileSizeKey, .contentModificationDateKey]
        
        guard let enumerator = fileManager.enumerator(at: folderURL.resolvingSymlinksInPath(),
                                                     includingPropertiesForKeys: resourceKeys,
                                                     options: [.skipsHiddenFiles, .skipsSubdirectoryDescendants]) else {
            throw ImageServiceError.cannotEnumerateFiles
        }
        
        var imageInfos: [ImageInfo] = []
        
        for case let fileURL as URL in enumerator {
            let fileExtension = fileURL.pathExtension.lowercased()
            guard Self.supportedExtensions.contains("." + fileExtension) else { continue }
            
            do {
                let resourceValues = try fileURL.resourceValues(forKeys: Set(resourceKeys))
                guard let isRegularFile = resourceValues.isRegularFile, isRegularFile else { continue }
                
                let fileSize = resourceValues.fileSize ?? 0
                let dateModified = resourceValues.contentModificationDate ?? Date()
                
                let imageInfo = ImageInfo(
                    filePath: fileURL.path,
                    fileName: fileURL.lastPathComponent,
                    fileSize: ByteCountFormatter.string(fromByteCount: Int64(fileSize), countStyle: .file),
                    dateModified: dateModified,
                    fileExtension: "." + fileExtension
                )
                
                imageInfos.append(imageInfo)
            } catch {
                print("Error reading file attributes for \(fileURL.path): \(error)")
            }
        }
        
        return imageInfos
    }
}

enum ImageServiceError: LocalizedError {
    case invalidFolder
    case cannotEnumerateFiles
    
    var errorDescription: String? {
        switch self {
        case .invalidFolder:
            return "The selected item is not a valid folder."
        case .cannotEnumerateFiles:
            return "Cannot access files in the selected folder."
        }
    }
}
