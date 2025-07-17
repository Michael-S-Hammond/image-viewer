//
//  ImageInfo.swift
//  ImageViewer-mac
//
//  Created by Michael Hammond on 7/16/25.
//

import Foundation

struct ImageInfo: Identifiable, Hashable {
    let id = UUID()
    let filePath: String
    let fileName: String
    let fileSize: String
    let dateModified: Date
    let fileExtension: String
    
    var isAnimatedGif: Bool {
        fileExtension.lowercased() == ".gif"
    }
    
    var isVideo: Bool {
        fileExtension.lowercased() == ".mp4"
    }
    
    var isStaticImage: Bool {
        !isAnimatedGif && !isVideo
    }
    
    var hasLoopableContent: Bool {
        isAnimatedGif || isVideo
    }
}