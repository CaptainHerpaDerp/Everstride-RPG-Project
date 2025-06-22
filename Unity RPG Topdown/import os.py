import os
import shutil

def rename_files_in_bulk(root_directory, source_keyword, target_keyword, duplicate=False):
    for dirpath, dirnames, filenames in os.walk(root_directory):
        # Duplicate folders if needed
        if duplicate:
            new_dirpath = dirpath.replace(source_keyword, target_keyword)
            if not os.path.exists(new_dirpath):
                os.makedirs(new_dirpath)

        for filename in filenames:
            if source_keyword in filename:
                new_filename = filename.replace(source_keyword, target_keyword)
                source_path = os.path.join(dirpath, filename)

                if duplicate:
                    target_path = os.path.join(new_dirpath, new_filename)
                    shutil.copy(source_path, target_path)
                else:
                    target_path = os.path.join(dirpath, new_filename)
                    os.rename(source_path, target_path)

        # Duplicate folder structure
        if duplicate and dirpath != new_dirpath:
            for subdir in dirnames:
                src_subdir_path = os.path.join(dirpath, subdir)
                target_subdir_path = os.path.join(new_dirpath, subdir.replace(source_keyword, target_keyword))
                if not os.path.exists(target_subdir_path):
                    shutil.copytree(src_subdir_path, target_subdir_path)

# Configure the script
root_dir = r"C:\Users\ruben\Dev\Unity-RPG-Topdown\Unity RPG Topdown\Assets\Animations\Cosmetics Animations\GlovesAnimations"  # Replace with your root directory path
source = "Torso"                        # The keyword to replace
target = "Gloves"                       # The replacement keyword
duplicate_folders = False                # Set to True if you want to duplicate folders

# Run the script
rename_files_in_bulk(root_dir, source, target, duplicate=duplicate_folders)
