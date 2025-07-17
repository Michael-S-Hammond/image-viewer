//
//  MediaDisplayView.swift
//  ImageViewer-mac
//
//  Created by Michael Hammond on 7/16/25.
//

import SwiftUI
import AVKit

struct MediaDisplayView: View {
    let imageInfo: ImageInfo
    let isLoopEnabled: Bool
    
    @State private var player: AVPlayer?
    @State private var nsImage: NSImage?
    
    var body: some View {
        ZStack {
            Color.black
            
            if imageInfo.isVideo {
                if let player = player {
                    VideoPlayer(player: player)
                        .onAppear {
                            setupVideoPlayer()
                        }
                        .onDisappear {
                            player.pause()
                        }
                } else {
                    ProgressView()
                        .onAppear {
                            setupVideoPlayer()
                        }
                }
            } else {
                if let nsImage = nsImage {
                    Image(nsImage: nsImage)
                        .resizable()
                        .aspectRatio(contentMode: .fit)
                } else {
                    ProgressView()
                        .onAppear {
                            loadImage()
                        }
                }
            }
        }
        .onChange(of: imageInfo) { _, _ in
            if imageInfo.isVideo {
                setupVideoPlayer()
            } else {
                loadImage()
            }
        }
    }
    
    private func setupVideoPlayer() {
        let url = URL(fileURLWithPath: imageInfo.filePath)
        let playerItem = AVPlayerItem(url: url)
        player = AVPlayer(playerItem: playerItem)
        player?.play()
        
        if isLoopEnabled {
            NotificationCenter.default.addObserver(
                forName: .AVPlayerItemDidPlayToEndTime,
                object: playerItem,
                queue: .main
            ) { _ in
                player?.seek(to: .zero)
                player?.play()
            }
        }
    }
    
    private func loadImage() {
        Task {
            let url = URL(fileURLWithPath: imageInfo.filePath)
            if let image = NSImage(contentsOf: url) {
                await MainActor.run {
                    self.nsImage = image
                }
            }
        }
    }
}

#Preview {
    let sampleImage = ImageInfo(
        filePath: "/System/Library/Desktop Pictures/Monterey.heic",
        fileName: "Monterey.heic",
        fileSize: "10 MB",
        dateModified: Date(),
        fileExtension: ".heic"
    )
    
    MediaDisplayView(imageInfo: sampleImage, isLoopEnabled: true)
}