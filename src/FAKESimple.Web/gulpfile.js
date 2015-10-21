/*
This file in the main entry point for defining Gulp tasks and using Gulp plugins.
Click here to learn more. http://go.microsoft.com/fwlink/?LinkId=518007
*/

var gulp = require('gulp');
var sourcemaps = require('gulp-sourcemaps');
var uglify = require('gulp-uglify');
var concat = require('gulp-concat');
var notifier = require('node-notifier');
var util = require('gulp-util');

var errorHandler = function(err) {
    notifier.notify({ message: 'Error: ' + err.message });
    util.log(util.colors.red('Error'), err.message);
};

var paths = {
    js: [
        "Scripts/jquery-1.10.2.js",
        "Scripts/jquery-validate.js",
        "Scripts/jquery-unobtrusive.js",
        "Scripts/bootstrap.js",
        "Scripts/modernizr-2.6.2.js",
        "Scripts/respond.js",
        "app/**/*.js"
    ],
    dist: 'dist'
}

gulp.task("js:app", function() {
    return gulp.src(paths.js)
        .pipe(sourcemaps.init())
        .on('error', function (err) {
            errorHandler(err);
            this.emit('end');
        })
        .pipe(concat('all.bundled.js'))
        .pipe(uglify())
        .pipe(sourcemaps.write())
        .pipe(gulp.dest(paths.dist));
});

gulp.task('default', function () {
    console.log("Hello world you");
});