// craco.config.js
const webpack = require("webpack")
const SimpleProgressWebpackPlugin = require('webpack-simple-progress-plugin')
const BundleAnalyzerPlugin = require("webpack-bundle-analyzer")
  .BundleAnalyzerPlugin

module.exports = function ({ env }) {
  const isProductionBuild = process.env.NODE_ENV === "production"
  const analyzerMode = process.env.REACT_APP_INTERACTIVE_ANALYZE
    ? "server"
    : "json"

  const plugins = [
    // 查看打包的进度
    new SimpleProgressWebpackPlugin()
  ]

  if (isProductionBuild) {
    plugins.push(new BundleAnalyzerPlugin({ analyzerMode }))
  }

  return {
    webpack: {
      plugins,
    },
  }
}