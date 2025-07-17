//
//  ThemeOption.swift
//  ImageViewer-mac
//
//  Created by Michael Hammond on 7/16/25.
//

import Foundation

enum ThemeOption: String, CaseIterable, Identifiable {
    case dark = "Dark"
    case light = "Light"
    
    var id: String { rawValue }
}