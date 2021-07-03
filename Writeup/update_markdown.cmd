@ECHO OFF
CD "%~dp0%"
TITLE Update Markdown

SET input=writeup.docx
SET output=README.md
SET media_dir=media

REM Checks if the input file exists
IF NOT EXIST %input% (
    ECHO Error: No file %input% in current directory
    ECHO Move or rename your file and try again
    GOTO CommonExit
)

REM Checks if Pandoc is on the path
REM /q for quiet (just sets ERRORLEVEL, no output)
where /q pandoc
IF %ERRORLEVEL% NEQ 0 (
    ECHO Error: Check Pandoc is installed
    ECHO Pandoc can be downloaded from https://pandoc.org/installing.html
    GOTO CommonExit
)

REM Empties old media directory if it exists
IF EXIST %media_dir% (
    REM /s for recursive, /q for quiet (no confirmation)
    rmdir /s /q %media_dir%
)

REM Runs the command to update the markdown
REM Outputs GitHub Favoured Markdown
pandoc %input% ^
    --standalone ^
    --extract-media . ^
    --to gfm-smart ^
    --wrap=none ^
    --reference-links ^
    -o %output%

REM When all is successful
ECHO Successfully updated %output%

:CommonExit
ECHO.
PAUSE
EXIT /b
