using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Represents a logical sentence.
 * 
 * Examples:
 * Person1 IS Alice TRUE
 * Alice HAS Knife TRUE
 * Alice HAS Knife FALSE
 * 
 * Ideally a group of sentences can either imply guilt or suspicion, or eliminate possibilities.
 */

public class Sentence
{
    Noun Subject;
    Verb Verb;
    Noun DirectObject;
    Adverb Adverb;
}
