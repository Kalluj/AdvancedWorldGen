import json
import os
import shutil


def make(language):
    file = open("Data.json", "r", encoding="utf8")
    options = json.load(file)
    file.close()

    translation = {}

    make_trans(language, options, translation)

    file = open(language + ".json", "w", encoding="utf8")
    json.dump(translation, file, indent=4, sort_keys=True, ensure_ascii=False)
    file.close()

    json_text = {}
    make_options(json_text, options)

    file = open("Options.json", "w", encoding="utf8")
    json.dump(json_text, file, indent=2, ensure_ascii=False)
    file.close()


def make_options(json_text, options):
    for key in options:
        option = options[key]
        o = {
            "Hidden": option["hidden"],
            "Conflicts": list(option["conflicts"].keys()),
            "Name": key
        }
        children = []
        make_options(children, option["children"])
        o["Children"] = children
        if isinstance(json_text, list):
            json_text.append(o)
        else:
            json_text[key] = o


def make_trans(language, options, translation, prefix=""):
    for key in options:
        option = options[key]

        name = option["displayed_name"]
        if type(name) is dict:
            if language not in name or name[language] == "":
                continue
            name = name[language]

        description = option["description"]
        if type(description) is dict:
            if language not in description or description[language] == "":
                continue
            description = description[language]

        conflicts = {}
        usual_translation = {"$parentVal": name, "Description": description, "Conflicts": conflicts}
        translation[key] = usual_translation
        conflicts_dict = option["conflicts"]
        for conflict_key in conflicts_dict:
            description = conflicts_dict[conflict_key]
            if type(description) is dict:
                if language not in description or description[language] == "":
                    continue
                description = description[language]

            split_key = conflict_key.split(".")
            internal_conflicts = conflicts
            for part_key in split_key:
                if part_key not in internal_conflicts:
                    internal_conflicts[part_key] = {}
                internal_conflicts = internal_conflicts[part_key]
            internal_conflicts["$parentVal"] = description

        if prefix == "":
            make_trans(language, option["children"], usual_translation, key)
        else:
            make_trans(language, option["children"], usual_translation, prefix + "." + key)


def translate_options(base_options, options, language, path=""):
    for key in options:
        if path == "":
            new_path = key
        else:
            new_path = path + "." + key
        option = options[key]
        option["displayed_name"] = translate(new_path + ".displayed_name", option["displayed_name"], language)
        save_data(base_options)
        option["description"] = translate(new_path + ".description", option["description"], language)
        save_data(base_options)

        for conflict_key in option["conflicts"]:
            option["conflicts"][conflict_key] = translate(new_path + ".conflict." + conflict_key,
                                                          option["conflicts"][conflict_key], language)

            address = conflict_key.split(".")
            parent = base_options
            for key2 in address:
                if parent == base_options:
                    parent = parent[key2]
                else:
                    parent = parent["children"][key2]

            parent["conflicts"][new_path] = option["conflicts"][conflict_key]
            save_data(base_options)

        translate_options(base_options, option["children"], language, new_path)


def save_data(options):
    file = open("Data.json", "w", encoding="utf8")
    json.dump(options, file, indent=4, sort_keys=True, ensure_ascii=False)
    file.close()


def translate(key, initial, language):
    if type(initial) is not dict:
        result = {"en-US": initial}
    elif language in initial and initial[language] != "":
        return initial
    else:
        result = initial

    print("key : " + key)
    print("english value : " + result["en-US"])
    input_text = input("translation : ")
    if input_text != "":
        result[language] = input_text

    return result


def make_files():
    make("en-US")
    make("fr-FR")
    make("ru-RU")
    make("zh-Hans")


def setup():
    shutil.copy("Options.json", "../Options.json")
    if os.path.isdir("../Localization"):
        shutil.rmtree("../Localization")
    os.mkdir(os.path.join("..", "Localization"))
    for folder_name in os.listdir("Localization"):
        folder_path = os.path.join("Localization", folder_name)
        new_folder_path = os.path.join("..", "Localization", folder_name, "Mods.AdvancedWorldGen")
        os.mkdir(os.path.join("..", "Localization", folder_name))
        shutil.copytree(folder_path, new_folder_path)
        shutil.copy(folder_name + ".json", os.path.join(new_folder_path, "Options.hjson"))
